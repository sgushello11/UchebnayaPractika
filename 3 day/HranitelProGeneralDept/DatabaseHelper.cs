using System;
using System.Data;
using System.Collections.Generic;
using Npgsql;

namespace HranitelProGeneralDept
{
    public class DatabaseHelper
    {
        private string connString = "Host=localhost;Port=5432;Database=HranitelPro;Username=postgres;Password=1";

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
            return Convert.ToDateTime(value);
        }

        // Авторизация по коду сотрудника
        public User? LoginByEmployeeCode(string code)
        {
            var dt = Query("SELECT * FROM employees WHERE employeecode = @code", new NpgsqlParameter("code", code));
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new User
            {
                EmployeeID = Convert.ToInt32(row["employeeid"]),
                LastName = row["lastname"]?.ToString() ?? "",
                FirstName = row["firstname"]?.ToString() ?? "",
                Patronymic = row["patronymic"]?.ToString(),
                EmployeeCode = row["employeecode"]?.ToString() ?? "",
                Department = row["department"]?.ToString() ?? ""
            };
        }

        // Получить все заявки
        public List<VisitRequest> GetAllRequests(string? status = null, string? requestType = null, string? department = null)
        {
            var list = new List<VisitRequest>();
            string sql = @"SELECT r.*, e.lastname || ' ' || e.firstname || COALESCE(' ' || e.patronymic, '') as employeename
                   FROM visitrequests r
                   LEFT JOIN employees e ON r.targetemployeeid = e.employeeid
                   WHERE 1=1";

            if (!string.IsNullOrEmpty(status) && status != "Все")
                sql += " AND r.status = @status";
            if (!string.IsNullOrEmpty(requestType) && requestType != "Все")
                sql += " AND r.requesttype = @type";
            if (!string.IsNullOrEmpty(department))
                sql += " AND r.targetdepartment = @dept";

            sql += " ORDER BY r.createdat DESC";

            using var conn = new NpgsqlConnection(connString);
            using var cmd = new NpgsqlCommand(sql, conn);
            if (!string.IsNullOrEmpty(status) && status != "Все")
                cmd.Parameters.AddWithValue("status", status);
            if (!string.IsNullOrEmpty(requestType) && requestType != "Все")
                cmd.Parameters.AddWithValue("type", requestType);
            if (!string.IsNullOrEmpty(department))
                cmd.Parameters.AddWithValue("dept", department);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new VisitRequest
                {
                    RequestID = reader.GetInt32(reader.GetOrdinal("requestid")),
                    RequestType = reader["requesttype"]?.ToString() ?? "",
                    Status = reader["status"]?.ToString() ?? "",
                    RejectionReason = reader["rejectionreason"]?.ToString() ?? "",
                    StartDate = ConvertToDateTime(reader["startdate"]),
                    EndDate = ConvertToDateTime(reader["enddate"]),
                    VisitPurpose = reader["visitpurpose"]?.ToString() ?? "",
                    TargetDepartment = reader["targetdepartment"]?.ToString() ?? "",
                    TargetEmployeeName = reader["employeename"]?.ToString() ?? "",
                    Note = reader["note"]?.ToString() ?? "",
                    CreatedAt = ConvertToDateTime(reader["createdat"]),
                    VisitorLastName = reader["visitor_lastname"]?.ToString() ?? "",
                    VisitorFirstName = reader["visitor_firstname"]?.ToString() ?? "",
                    VisitorPatronymic = reader["visitor_patronymic"]?.ToString() ?? "",
                    VisitorPhone = reader["visitor_phone"]?.ToString() ?? "",
                    VisitorEmail = reader["visitor_email"]?.ToString() ?? "",
                    VisitorOrganization = reader["visitor_organization"]?.ToString() ?? "",
                    VisitorBirthDate = reader["visitor_birthdate"] == DBNull.Value ? DateTime.Now : ConvertToDateTime(reader["visitor_birthdate"]),
                    VisitorPassportData = reader["visitor_passportdata"]?.ToString() ?? ""
                });
            }
            return list;
        }

        // Получить заявку по ID
        public VisitRequest? GetRequestById(int requestId)
        {
            var dt = Query(@"SELECT r.*, e.lastname || ' ' || e.firstname || COALESCE(' ' || e.patronymic, '') as employeename
                            FROM visitrequests r
                            LEFT JOIN employees e ON r.targetemployeeid = e.employeeid
                            WHERE r.requestid = @id", new NpgsqlParameter("id", requestId));
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new VisitRequest
            {
                RequestID = Convert.ToInt32(row["requestid"]),
                RequestType = row["requesttype"]?.ToString() ?? "",
                Status = row["status"]?.ToString() ?? "",
                RejectionReason = row["rejectionreason"]?.ToString(),
                StartDate = ConvertToDateTime(row["startdate"]),
                EndDate = ConvertToDateTime(row["enddate"]),
                VisitPurpose = row["visitpurpose"]?.ToString() ?? "",
                TargetDepartment = row["targetdepartment"]?.ToString() ?? "",
                TargetEmployeeName = row["employeename"]?.ToString() ?? "",
                Note = row["note"]?.ToString() ?? "",
                CreatedAt = ConvertToDateTime(row["createdat"]),
                VisitorLastName = row["visitor_lastname"]?.ToString() ?? "",
                VisitorFirstName = row["visitor_firstname"]?.ToString() ?? "",
                VisitorPatronymic = row["visitor_patronymic"]?.ToString() ?? "",
                VisitorPhone = row["visitor_phone"]?.ToString() ?? "",
                VisitorEmail = row["visitor_email"]?.ToString() ?? "",
                VisitorOrganization = row["visitor_organization"]?.ToString() ?? "",
                VisitorBirthDate = row["visitor_birthdate"] == DBNull.Value ? DateTime.Now : ConvertToDateTime(row["visitor_birthdate"]),
                VisitorPassportData = row["visitor_passportdata"]?.ToString() ?? ""
            };
        }

        // Проверка черного списка
        public bool IsInBlackList(string passportData)
        {
            var dt = Query("SELECT COUNT(*) FROM blacklist WHERE passportdata = @pd", new NpgsqlParameter("pd", passportData));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        // Обновить статус заявки
        public int UpdateRequestStatus(int requestId, string status, string? rejectionReason = null, DateTime? visitDate = null, TimeSpan? visitTime = null)
        {
            string sql;
            if (status == "Одобрена")
            {
                sql = @"UPDATE visitrequests SET status = @status, rejectionreason = NULL, startdate = @date 
                        WHERE requestid = @id";
                return Execute(sql,
                    new NpgsqlParameter("status", status),
                    new NpgsqlParameter("date", visitDate ?? DateTime.Now),
                    new NpgsqlParameter("id", requestId));
            }
            else
            {
                sql = @"UPDATE visitrequests SET status = @status, rejectionreason = @reason 
                        WHERE requestid = @id";
                return Execute(sql,
                    new NpgsqlParameter("status", status),
                    new NpgsqlParameter("reason", rejectionReason ?? ""),
                    new NpgsqlParameter("id", requestId));
            }
        }

        // Получить уникальные подразделения
        public List<string> GetDepartments()
        {
            var list = new List<string>();
            var dt = Query("SELECT DISTINCT targetdepartment FROM visitrequests WHERE targetdepartment IS NOT NULL ORDER BY targetdepartment");
            foreach (DataRow row in dt.Rows)
            {
                list.Add(row["targetdepartment"]?.ToString() ?? "");
            }
            return list;
        }

        // Добавить в черный список
        public int AddToBlackList(string lastName, string firstName, string? patronymic, string passportData)
        {
            string sql = @"INSERT INTO blacklist (lastname, firstname, patronymic, passportdata, addedat) 
                   VALUES (@ln, @fn, @pat, @pd, @date)";
            return Execute(sql,
                new NpgsqlParameter("ln", lastName),
                new NpgsqlParameter("fn", firstName),
                new NpgsqlParameter("pat", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic),
                new NpgsqlParameter("pd", passportData),
                new NpgsqlParameter("date", DateTime.Now));
        }

        // Получить количество отклонений по паспорту
        public int GetRejectionCountByPassport(string passportData)
        {
            if (string.IsNullOrEmpty(passportData)) return 0;
            var dt = Query("SELECT COUNT(*) FROM visitrequests WHERE visitor_passportdata = @pd AND status = 'не одобрена' AND rejectionreason LIKE '%недостоверных%'",
                new NpgsqlParameter("pd", passportData));
            return Convert.ToInt32(dt.Rows[0][0]);
        }

        // Получить прикреплённые файлы по ID заявки
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
    }
}