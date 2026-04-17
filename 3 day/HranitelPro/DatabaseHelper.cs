using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace HranitelPro
{
    public class DatabaseHelper
    {
        private string connString = "Host=localhost;Port=5432;Database=HranitelPro;Username=postgres;Password=1";

        public string HashMD5(string input)
        {
            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private DataTable Query(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = new NpgsqlConnection(connString);
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            var dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            return dt;
        }

        private int Execute(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = new NpgsqlConnection(connString);
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null) cmd.Parameters.AddRange(parameters);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        private DateTime ConvertToDateTime(object value)
        {
            if (value is DateTime dt) return dt;
            if (value is DateOnly dateOnly) return dateOnly.ToDateTime(TimeOnly.MinValue);
            if (value is string str) return DateTime.Parse(str);
            return Convert.ToDateTime(value);
        }

        // ==================== АВТОРИЗАЦИЯ ====================
        public bool LoginSQL(string login, string hash)
        {
            var dt = Query("SELECT COUNT(*) FROM users WHERE login=@l AND passwordhash=@p",
                new NpgsqlParameter("l", login), new NpgsqlParameter("p", hash));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        public User? LoginORM(string login, string hash)
        {
            var dt = Query("SELECT * FROM users WHERE login=@l AND passwordhash=@p",
                new NpgsqlParameter("l", login), new NpgsqlParameter("p", hash));
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new User
            {
                UserID = Convert.ToInt32(row["userid"]),
                LastName = row["lastname"]?.ToString() ?? "",
                FirstName = row["firstname"]?.ToString() ?? "",
                Patronymic = row["patronymic"]?.ToString(),
                Login = row["login"]?.ToString() ?? ""
            };
        }

        // ==================== РЕГИСТРАЦИЯ ====================
        public bool RegisterSQL(string last, string first, string? patron, string? phone, string email,
                          DateTime birth, string passport, string login, string hash)
        {
            string sql = @"INSERT INTO users (lastname, firstname, patronymic, phone, email, birthdate, passportdata, login, passwordhash) 
                           VALUES (@ln, @fn, @pat, @ph, @em, @bd, @pd, @l, @pwd)";
            return Execute(sql,
                new NpgsqlParameter("ln", last),
                new NpgsqlParameter("fn", first),
                new NpgsqlParameter("pat", string.IsNullOrEmpty(patron) ? DBNull.Value : (object)patron),
                new NpgsqlParameter("ph", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone),
                new NpgsqlParameter("em", email),
                new NpgsqlParameter("bd", birth),
                new NpgsqlParameter("pd", passport),
                new NpgsqlParameter("l", login),
                new NpgsqlParameter("pwd", hash)) > 0;
        }

        // ==================== ЗАЯВКИ ====================
        public List<RequestItem> GetUserRequests(int userId)
        {
            var list = new List<RequestItem>();
            var dt = Query(@"SELECT requestid, requesttype, status, startdate, enddate, visitpurpose, targetdepartment, createdat
                            FROM visitrequests WHERE userid=@uid ORDER BY createdat DESC",
                            new NpgsqlParameter("uid", userId));
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new RequestItem
                {
                    Id = Convert.ToInt32(row["requestid"]),
                    Type = row["requesttype"]?.ToString() ?? "",
                    Status = row["status"]?.ToString() ?? "",
                    StartDate = ConvertToDateTime(row["startdate"]).ToShortDateString(),
                    EndDate = ConvertToDateTime(row["enddate"]).ToShortDateString(),
                    Purpose = row["visitpurpose"]?.ToString() ?? "",
                    Department = row["targetdepartment"]?.ToString() ?? "",
                    CreatedAt = ConvertToDateTime(row["createdat"]).ToShortDateString()
                });
            }
            return list;
        }

        public RequestFull? GetRequestById(int requestId)
        {
            var dt = Query(@"SELECT requestid, requesttype, status, startdate, enddate, visitpurpose, targetdepartment, note,
                           visitor_lastname, visitor_firstname, visitor_patronymic, visitor_phone, visitor_email,
                           visitor_organization, visitor_birthdate, visitor_passportdata
                    FROM visitrequests WHERE requestid=@id",
                            new NpgsqlParameter("id", requestId));
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new RequestFull
            {
                RequestID = Convert.ToInt32(row["requestid"]),
                RequestType = row["requesttype"]?.ToString() ?? "",
                Status = row["status"]?.ToString() ?? "",
                StartDate = ConvertToDateTime(row["startdate"]),
                EndDate = ConvertToDateTime(row["enddate"]),
                VisitPurpose = row["visitpurpose"]?.ToString() ?? "",
                TargetDepartment = row["targetdepartment"]?.ToString() ?? "",
                Note = row["note"]?.ToString() ?? "",
                VisitorLastName = row["visitor_lastname"]?.ToString() ?? "",
                VisitorFirstName = row["visitor_firstname"]?.ToString() ?? "",
                VisitorPatronymic = row["visitor_patronymic"]?.ToString(),
                VisitorPhone = row["visitor_phone"]?.ToString(),
                VisitorEmail = row["visitor_email"]?.ToString() ?? "",
                VisitorOrganization = row["visitor_organization"]?.ToString(),
                VisitorBirthDate = row["visitor_birthdate"] == DBNull.Value ? DateTime.Now : ConvertToDateTime(row["visitor_birthdate"]),
                VisitorPassportData = row["visitor_passportdata"]?.ToString() ?? ""
            };
        }

        public int CreateRequest(int userId, DateTime start, DateTime end, string purpose, string dept, int empId, string note,
                          string lastName, string firstName, string? patronymic, string? phone, string email,
                          string? organization, DateTime birthDate, string passportData)
        {
            string sql = @"INSERT INTO visitrequests (userid, requesttype, status, startdate, enddate, 
                   visitpurpose, targetdepartment, targetemployeeid, note,
                   visitor_lastname, visitor_firstname, visitor_patronymic, visitor_phone, visitor_email,
                   visitor_organization, visitor_birthdate, visitor_passportdata)
                   VALUES (@uid, 'личная', 'проверка', @start, @end, @purpose, @dept, @emp, @note,
                           @ln, @fn, @pat, @ph, @em, @org, @bd, @pd) RETURNING requestid";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("start", start);
            cmd.Parameters.AddWithValue("end", end);
            cmd.Parameters.AddWithValue("purpose", purpose);
            cmd.Parameters.AddWithValue("dept", dept);
            cmd.Parameters.AddWithValue("emp", empId);
            cmd.Parameters.AddWithValue("note", note);
            cmd.Parameters.AddWithValue("ln", lastName);
            cmd.Parameters.AddWithValue("fn", firstName);
            cmd.Parameters.AddWithValue("pat", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic);
            cmd.Parameters.AddWithValue("ph", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
            cmd.Parameters.AddWithValue("em", email);
            cmd.Parameters.AddWithValue("org", string.IsNullOrEmpty(organization) ? DBNull.Value : (object)organization);
            cmd.Parameters.AddWithValue("bd", birthDate);
            cmd.Parameters.AddWithValue("pd", passportData);

            return Convert.ToInt32(cmd.ExecuteScalar()); // Возвращаем ID новой заявки
        }

        public int UpdateRequest(int requestId, DateTime start, DateTime end, string purpose, string dept, int empId, string note,
                          string lastName, string firstName, string? patronymic, string? phone, string email,
                          string? organization, DateTime birthDate, string passportData)
        {
            string sql = @"UPDATE visitrequests SET 
                   startdate=@start, enddate=@end, visitpurpose=@purpose, 
                   targetdepartment=@dept, targetemployeeid=@emp, note=@note,
                   visitor_lastname=@ln, visitor_firstname=@fn, visitor_patronymic=@pat,
                   visitor_phone=@ph, visitor_email=@em, visitor_organization=@org,
                   visitor_birthdate=@bd, visitor_passportdata=@pd
                   WHERE requestid=@id";

            Execute(sql,
                new NpgsqlParameter("start", start),
                new NpgsqlParameter("end", end),
                new NpgsqlParameter("purpose", purpose),
                new NpgsqlParameter("dept", dept),
                new NpgsqlParameter("emp", empId),
                new NpgsqlParameter("note", note),
                new NpgsqlParameter("ln", lastName),
                new NpgsqlParameter("fn", firstName),
                new NpgsqlParameter("pat", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic),
                new NpgsqlParameter("ph", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone),
                new NpgsqlParameter("em", email),
                new NpgsqlParameter("org", string.IsNullOrEmpty(organization) ? DBNull.Value : (object)organization),
                new NpgsqlParameter("bd", birthDate),
                new NpgsqlParameter("pd", passportData),
                new NpgsqlParameter("id", requestId));

            return requestId; // Возвращаем ID обновлённой заявки
        }

        public int DeleteRequest(int requestId)
        {
            return Execute("DELETE FROM visitrequests WHERE requestid=@id", new NpgsqlParameter("id", requestId));
        }

        // Создание групповой заявки с данными организатора
        public int CreateGroupRequest(int userId, DateTime start, DateTime end, string purpose, string dept, int empId, string note,
                                        string lastName, string firstName, string? patronymic, string? phone, string email,
                                        string? organization, DateTime birthDate, string passportData)
        {
            string sql = @"INSERT INTO visitrequests (userid, requesttype, status, startdate, enddate, 
                   visitpurpose, targetdepartment, targetemployeeid, note,
                   visitor_lastname, visitor_firstname, visitor_patronymic, visitor_phone, visitor_email,
                   visitor_organization, visitor_birthdate, visitor_passportdata)
                   VALUES (@uid, 'групповая', 'проверка', @start, @end, @purpose, @dept, @emp, @note,
                           @ln, @fn, @pat, @ph, @em, @org, @bd, @pd)";
            return Execute(sql,
                new NpgsqlParameter("uid", userId),
                new NpgsqlParameter("start", start),
                new NpgsqlParameter("end", end),
                new NpgsqlParameter("purpose", purpose),
                new NpgsqlParameter("dept", dept),
                new NpgsqlParameter("emp", empId),
                new NpgsqlParameter("note", note),
                new NpgsqlParameter("ln", lastName),
                new NpgsqlParameter("fn", firstName),
                new NpgsqlParameter("pat", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic),
                new NpgsqlParameter("ph", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone),
                new NpgsqlParameter("em", email),
                new NpgsqlParameter("org", string.IsNullOrEmpty(organization) ? DBNull.Value : (object)organization),
                new NpgsqlParameter("bd", birthDate),
                new NpgsqlParameter("pd", passportData));
        }

        // Обновление групповой заявки
        public int UpdateGroupRequest(int requestId, DateTime start, DateTime end, string purpose, string dept, int empId, string note,
                                        string lastName, string firstName, string? patronymic, string? phone, string email,
                                        string? organization, DateTime birthDate, string passportData)
        {
            string sql = @"UPDATE visitrequests SET 
                   startdate=@start, enddate=@end, visitpurpose=@purpose, 
                   targetdepartment=@dept, targetemployeeid=@emp, note=@note,
                   visitor_lastname=@ln, visitor_firstname=@fn, visitor_patronymic=@pat,
                   visitor_phone=@ph, visitor_email=@em, visitor_organization=@org,
                   visitor_birthdate=@bd, visitor_passportdata=@pd
                   WHERE requestid=@id";
            return Execute(sql,
                new NpgsqlParameter("start", start),
                new NpgsqlParameter("end", end),
                new NpgsqlParameter("purpose", purpose),
                new NpgsqlParameter("dept", dept),
                new NpgsqlParameter("emp", empId),
                new NpgsqlParameter("note", note),
                new NpgsqlParameter("ln", lastName),
                new NpgsqlParameter("fn", firstName),
                new NpgsqlParameter("pat", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic),
                new NpgsqlParameter("ph", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone),
                new NpgsqlParameter("em", email),
                new NpgsqlParameter("org", string.IsNullOrEmpty(organization) ? DBNull.Value : (object)organization),
                new NpgsqlParameter("bd", birthDate),
                new NpgsqlParameter("pd", passportData),
                new NpgsqlParameter("id", requestId));
        }

        // Получить членов группы по ID заявки
        public List<GroupMember> GetGroupMembersByRequestId(int requestId)
        {
            var list = new List<GroupMember>();

            // Получаем groupvisitid
            string getGroupSql = "SELECT groupvisitid FROM groupvisits WHERE requestid = @rid";
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var getGroupCmd = new NpgsqlCommand(getGroupSql, conn);
            getGroupCmd.Parameters.AddWithValue("rid", requestId);
            object result = getGroupCmd.ExecuteScalar();

            if (result == null) return list;

            int groupVisitId = Convert.ToInt32(result);

            // Получаем членов группы
            var dt = Query("SELECT * FROM groupmembers WHERE groupvisitid = @gid", new NpgsqlParameter("gid", groupVisitId));
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new GroupMember
                {
                    LastName = row["lastname"]?.ToString() ?? "",
                    FirstName = row["firstname"]?.ToString() ?? "",
                    Patronymic = row["patronymic"]?.ToString(),
                    Phone = row["phone"]?.ToString(),
                    Email = row["email"]?.ToString() ?? "",
                    BirthDate = ConvertToDateTime(row["birthdate"]),
                    PassportData = row["passportdata"]?.ToString() ?? ""
                });
            }
            return list;
        }

        public int CreateGroupRequestWithMembers(int userId, DateTime start, DateTime end, string purpose, string dept, int empId, string note,
                                           string lastName, string firstName, string? patronymic, string? phone, string email,
                                           string? organization, DateTime birthDate, string passportData, string? passportFile,
                                           ObservableCollection<GroupMember> members)
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                // 1. Вставка заявки
                string sql = @"INSERT INTO visitrequests (userid, requesttype, status, startdate, enddate, 
                       visitpurpose, targetdepartment, targetemployeeid, note,
                       visitor_lastname, visitor_firstname, visitor_patronymic, visitor_phone, visitor_email,
                       visitor_organization, visitor_birthdate, visitor_passportdata)
                       VALUES (@uid, 'групповая', 'проверка', @start, @end, @purpose, @dept, @emp, @note,
                               @ln, @fn, @pat, @ph, @em, @org, @bd, @pd) RETURNING requestid";

                using var cmd = new NpgsqlCommand(sql, conn, trans);
                cmd.Parameters.AddWithValue("uid", userId);
                cmd.Parameters.AddWithValue("start", start);
                cmd.Parameters.AddWithValue("end", end);
                cmd.Parameters.AddWithValue("purpose", purpose);
                cmd.Parameters.AddWithValue("dept", dept);
                cmd.Parameters.AddWithValue("emp", empId);
                cmd.Parameters.AddWithValue("note", note);
                cmd.Parameters.AddWithValue("ln", lastName);
                cmd.Parameters.AddWithValue("fn", firstName);
                cmd.Parameters.AddWithValue("pat", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic);
                cmd.Parameters.AddWithValue("ph", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                cmd.Parameters.AddWithValue("em", email);
                cmd.Parameters.AddWithValue("org", string.IsNullOrEmpty(organization) ? DBNull.Value : (object)organization);
                cmd.Parameters.AddWithValue("bd", birthDate);
                cmd.Parameters.AddWithValue("pd", passportData);

                int requestId = Convert.ToInt32(cmd.ExecuteScalar());

                // 2. Создание записи в groupvisits
                string groupSql = @"INSERT INTO groupvisits (requestid) VALUES (@rid) RETURNING groupvisitid";
                using var groupCmd = new NpgsqlCommand(groupSql, conn, trans);
                groupCmd.Parameters.AddWithValue("rid", requestId);
                int groupVisitId = Convert.ToInt32(groupCmd.ExecuteScalar());

                // 3. Добавление файла
                if (!string.IsNullOrEmpty(passportFile))
                {
                    string fileSql = "INSERT INTO attachedfiles (requestid, filetype, filepath, filename) VALUES (@rid, 'passport_scan', @path, @name)";
                    using var fileCmd = new NpgsqlCommand(fileSql, conn, trans);
                    fileCmd.Parameters.AddWithValue("rid", requestId);
                    fileCmd.Parameters.AddWithValue("path", passportFile);
                    fileCmd.Parameters.AddWithValue("name", System.IO.Path.GetFileName(passportFile));
                    fileCmd.ExecuteNonQuery();
                }

                // 4. Добавление членов группы
                foreach (var member in members)
                {
                    string memberSql = @"INSERT INTO groupmembers (groupvisitid, lastname, firstname, patronymic, phone, email, birthdate, passportdata) 
                                 VALUES (@gid, @ln, @fn, @pat, @ph, @em, @bd, @pd)";
                    using var memberCmd = new NpgsqlCommand(memberSql, conn, trans);
                    memberCmd.Parameters.AddWithValue("gid", groupVisitId);
                    memberCmd.Parameters.AddWithValue("ln", member.LastName);
                    memberCmd.Parameters.AddWithValue("fn", member.FirstName);
                    memberCmd.Parameters.AddWithValue("pat", string.IsNullOrEmpty(member.Patronymic) ? DBNull.Value : (object)member.Patronymic);
                    memberCmd.Parameters.AddWithValue("ph", string.IsNullOrEmpty(member.Phone) ? DBNull.Value : (object)member.Phone);
                    memberCmd.Parameters.AddWithValue("em", member.Email);
                    memberCmd.Parameters.AddWithValue("bd", member.BirthDate);
                    memberCmd.Parameters.AddWithValue("pd", member.PassportData);
                    memberCmd.ExecuteNonQuery();
                }

                trans.Commit();
                return requestId;
            }
            catch
            {
                trans.Rollback();
                return 0;
            }
        }

        // Обновление групповой заявки с членами группы
        public int UpdateGroupRequestWithMembers(int requestId, DateTime start, DateTime end, string purpose, string dept, int empId, string note,
                                           string lastName, string firstName, string? patronymic, string? phone, string email,
                                           string? organization, DateTime birthDate, string passportData, string? passportFile,
                                           ObservableCollection<GroupMember> members, int? existingPassportFileId)
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            using var trans = conn.BeginTransaction();
            try
            {
                // 1. Обновление заявки
                string sql = @"UPDATE visitrequests SET 
                       startdate=@start, enddate=@end, visitpurpose=@purpose, 
                       targetdepartment=@dept, targetemployeeid=@emp, note=@note,
                       visitor_lastname=@ln, visitor_firstname=@fn, visitor_patronymic=@pat,
                       visitor_phone=@ph, visitor_email=@em, visitor_organization=@org,
                       visitor_birthdate=@bd, visitor_passportdata=@pd
                       WHERE requestid=@id";

                using var cmd = new NpgsqlCommand(sql, conn, trans);
                cmd.Parameters.AddWithValue("start", start);
                cmd.Parameters.AddWithValue("end", end);
                cmd.Parameters.AddWithValue("purpose", purpose);
                cmd.Parameters.AddWithValue("dept", dept);
                cmd.Parameters.AddWithValue("emp", empId);
                cmd.Parameters.AddWithValue("note", note);
                cmd.Parameters.AddWithValue("ln", lastName);
                cmd.Parameters.AddWithValue("fn", firstName);
                cmd.Parameters.AddWithValue("pat", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic);
                cmd.Parameters.AddWithValue("ph", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone);
                cmd.Parameters.AddWithValue("em", email);
                cmd.Parameters.AddWithValue("org", string.IsNullOrEmpty(organization) ? DBNull.Value : (object)organization);
                cmd.Parameters.AddWithValue("bd", birthDate);
                cmd.Parameters.AddWithValue("pd", passportData);
                cmd.Parameters.AddWithValue("id", requestId);
                cmd.ExecuteNonQuery();

                // 2. Получение groupvisitid из таблицы groupvisits
                string getGroupSql = "SELECT groupvisitid FROM groupvisits WHERE requestid = @rid";
                using var getGroupCmd = new NpgsqlCommand(getGroupSql, conn, trans);
                getGroupCmd.Parameters.AddWithValue("rid", requestId);
                object result = getGroupCmd.ExecuteScalar();

                if (result == null)
                {
                    // Если нет записи в groupvisits, создаём новую
                    string insertGroupSql = "INSERT INTO groupvisits (requestid) VALUES (@rid) RETURNING groupvisitid";
                    using var insertGroupCmd = new NpgsqlCommand(insertGroupSql, conn, trans);
                    insertGroupCmd.Parameters.AddWithValue("rid", requestId);
                    result = insertGroupCmd.ExecuteScalar();
                }

                int groupVisitId = Convert.ToInt32(result);

                // 3. Обновление файла
                if (existingPassportFileId.HasValue && string.IsNullOrEmpty(passportFile))
                {
                    string delSql = "DELETE FROM attachedfiles WHERE fileid = @fid";
                    using var delCmd = new NpgsqlCommand(delSql, conn, trans);
                    delCmd.Parameters.AddWithValue("fid", existingPassportFileId.Value);
                    delCmd.ExecuteNonQuery();
                }
                else if (!string.IsNullOrEmpty(passportFile) && !existingPassportFileId.HasValue)
                {
                    string fileSql = "INSERT INTO attachedfiles (requestid, filetype, filepath, filename) VALUES (@rid, 'passport_scan', @path, @name)";
                    using var fileCmd = new NpgsqlCommand(fileSql, conn, trans);
                    fileCmd.Parameters.AddWithValue("rid", requestId);
                    fileCmd.Parameters.AddWithValue("path", passportFile);
                    fileCmd.Parameters.AddWithValue("name", System.IO.Path.GetFileName(passportFile));
                    fileCmd.ExecuteNonQuery();
                }

                // 4. Обновление членов группы (удаляем старых, добавляем новых)
                string delMembersSql = "DELETE FROM groupmembers WHERE groupvisitid = @gid";
                using var delMembersCmd = new NpgsqlCommand(delMembersSql, conn, trans);
                delMembersCmd.Parameters.AddWithValue("gid", groupVisitId);
                delMembersCmd.ExecuteNonQuery();

                foreach (var member in members)
                {
                    string memberSql = @"INSERT INTO groupmembers (groupvisitid, lastname, firstname, patronymic, phone, email, birthdate, passportdata) 
                                 VALUES (@gid, @ln, @fn, @pat, @ph, @em, @bd, @pd)";
                    using var memberCmd = new NpgsqlCommand(memberSql, conn, trans);
                    memberCmd.Parameters.AddWithValue("gid", groupVisitId);
                    memberCmd.Parameters.AddWithValue("ln", member.LastName);
                    memberCmd.Parameters.AddWithValue("fn", member.FirstName);
                    memberCmd.Parameters.AddWithValue("pat", string.IsNullOrEmpty(member.Patronymic) ? DBNull.Value : (object)member.Patronymic);
                    memberCmd.Parameters.AddWithValue("ph", string.IsNullOrEmpty(member.Phone) ? DBNull.Value : (object)member.Phone);
                    memberCmd.Parameters.AddWithValue("em", member.Email);
                    memberCmd.Parameters.AddWithValue("bd", member.BirthDate);
                    memberCmd.Parameters.AddWithValue("pd", member.PassportData);
                    memberCmd.ExecuteNonQuery();
                }

                trans.Commit();
                return requestId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                MessageBox.Show($"Ошибка: {ex.Message}");
                return 0;
            }
        }

        // ==================== СПРАВОЧНИКИ ====================
        public DataTable GetDepartments()
        {
            return Query("SELECT DISTINCT department FROM employees WHERE department IS NOT NULL ORDER BY department");
        }

        public DataTable GetEmployees(string department)
        {
            return Query(@"SELECT employeeid, lastname || ' ' || firstname || COALESCE(' ' || patronymic, '') as fullname
                           FROM employees WHERE department=@dept ORDER BY lastname",
                           new NpgsqlParameter("dept", department));
        }

        // ==================== ФАЙЛЫ ====================

        // Получить файлы по ID заявки
        public List<AttachedFile> GetAttachedFiles(int requestId)
        {
            var list = new List<AttachedFile>();
            var dt = Query("SELECT * FROM attachedfiles WHERE requestid = @id", new NpgsqlParameter("id", requestId));
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new AttachedFile
                {
                    FileId = Convert.ToInt32(row["fileid"]),
                    FileType = row["filetype"]?.ToString() ?? "",
                    FilePath = row["filepath"]?.ToString() ?? "",
                    FileName = row["filename"]?.ToString() ?? ""
                });
            }
            return list;
        }

        // Добавить файл к заявке
        public int AddAttachedFile(int requestId, string fileType, string filePath, string fileName)
        {
            string sql = @"INSERT INTO attachedfiles (requestid, filetype, filepath, filename) 
                   VALUES (@rid, @type, @path, @name)";
            return Execute(sql,
                new NpgsqlParameter("rid", requestId),
                new NpgsqlParameter("type", fileType),
                new NpgsqlParameter("path", filePath),
                new NpgsqlParameter("name", fileName));
        }

        // Удалить файл
        public int DeleteAttachedFile(int fileId)
        {
            return Execute("DELETE FROM attachedfiles WHERE fileid = @id", new NpgsqlParameter("id", fileId));
        }

        // Удалить все файлы заявки
        public int DeleteAllAttachedFiles(int requestId)
        {
            return Execute("DELETE FROM attachedfiles WHERE requestid = @rid", new NpgsqlParameter("rid", requestId));
        }
    }
}