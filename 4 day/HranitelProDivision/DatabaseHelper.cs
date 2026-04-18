using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace HranitelProDivision
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

        // Получить одобренные заявки по подразделению сотрудника
        public List<VisitRequest> GetApprovedRequestsByDepartment(string department)
        {
            var list = new List<VisitRequest>();
            var dt = Query("SELECT * FROM visitrequests WHERE status = 'одобрена' AND targetdepartment = @dept ORDER BY startdate",
                new NpgsqlParameter("dept", department));

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new VisitRequest
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
                    VisitorPatronymic = row["visitor_patronymic"]?.ToString() ?? "",
                    VisitorPhone = row["visitor_phone"]?.ToString() ?? "",
                    VisitorEmail = row["visitor_email"]?.ToString() ?? "",
                    VisitorOrganization = row["visitor_organization"]?.ToString() ?? "",
                    VisitorBirthDate = row["visitor_birthdate"] == DBNull.Value ? DateTime.Now : ConvertToDateTime(row["visitor_birthdate"]),
                    VisitorPassportData = row["visitor_passportdata"]?.ToString() ?? "",
                    ActualEntryTime = row["actual_entry_time"] == DBNull.Value ? null : ConvertToDateTime(row["actual_entry_time"]),
                    DivisionEntryTime = row["division_entry_time"] == DBNull.Value ? null : ConvertToDateTime(row["division_entry_time"]),
                    ActualExitTime = row["actual_exit_time"] == DBNull.Value ? null : ConvertToDateTime(row["actual_exit_time"])
                });
            }
            return list;
        }

        // Получить заявку по ID
        public VisitRequest? GetRequestById(int requestId)
        {
            var dt = Query("SELECT * FROM visitrequests WHERE requestid = @id", new NpgsqlParameter("id", requestId));
            if (dt.Rows.Count == 0) return null;
            var row = dt.Rows[0];
            return new VisitRequest
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
                VisitorPatronymic = row["visitor_patronymic"]?.ToString() ?? "",
                VisitorPhone = row["visitor_phone"]?.ToString() ?? "",
                VisitorEmail = row["visitor_email"]?.ToString() ?? "",
                VisitorOrganization = row["visitor_organization"]?.ToString() ?? "",
                VisitorBirthDate = row["visitor_birthdate"] == DBNull.Value ? DateTime.Now : ConvertToDateTime(row["visitor_birthdate"]),
                VisitorPassportData = row["visitor_passportdata"]?.ToString() ?? "",
                ActualEntryTime = row["actual_entry_time"] == DBNull.Value ? null : ConvertToDateTime(row["actual_entry_time"]),
                ActualExitTime = row["actual_exit_time"] == DBNull.Value ? null : ConvertToDateTime(row["actual_exit_time"])
            };
        }

        // Зафиксировать вход сотрудником подразделения
        public int SetDivisionEntryTime(int requestId, DateTime entryTime)
        {
            return Execute("UPDATE visitrequests SET division_entry_time = @time WHERE requestid = @id",
                new NpgsqlParameter("time", entryTime),
                new NpgsqlParameter("id", requestId));
        }

        // Зафиксировать выход сотрудником подразделения
        public int SetExitTime(int requestId, DateTime exitTime)
        {
            return Execute("UPDATE visitrequests SET actual_exit_time = @time WHERE requestid = @id",
                new NpgsqlParameter("time", exitTime),
                new NpgsqlParameter("id", requestId));
        }

        // Добавить в черный список
        public int AddToBlacklist(string lastName, string firstName, string? patronymic, string passportData, string reason)
        {
            string sql = @"INSERT INTO blacklist (lastname, firstname, patronymic, passportdata, reason, addedat) 
                           VALUES (@ln, @fn, @pat, @pd, @reason, @date)";
            return Execute(sql,
                new NpgsqlParameter("ln", lastName),
                new NpgsqlParameter("fn", firstName),
                new NpgsqlParameter("pat", string.IsNullOrEmpty(patronymic) ? DBNull.Value : (object)patronymic),
                new NpgsqlParameter("pd", passportData),
                new NpgsqlParameter("reason", reason),
                new NpgsqlParameter("date", DateTime.Now));
        }


        // Удалить из черного списка
        public int RemoveFromBlacklist(string passportData)
        {
            return Execute("DELETE FROM blacklist WHERE passportdata = @pd", new NpgsqlParameter("pd", passportData));
        }

        // Проверка черного списка
        public bool IsInBlacklist(string passportData)
        {
            var dt = Query("SELECT COUNT(*) FROM blacklist WHERE passportdata = @pd", new NpgsqlParameter("pd", passportData));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        // Получить причину добавления в черный список
        public string GetBlacklistReason(string passportData)
        {
            var dt = Query("SELECT reason FROM blacklist WHERE passportdata = @pd", new NpgsqlParameter("pd", passportData));
            if (dt.Rows.Count == 0) return "";
            return dt.Rows[0]["reason"]?.ToString() ?? "";
        }
    }
}