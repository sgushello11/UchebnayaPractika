using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace HranitelProSecurity
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

        public List<VisitRequest> GetApprovedRequests(string? date = null, string? type = null, string? department = null, string? searchText = null)
        {
            var list = new List<VisitRequest>();
            string sql = @"SELECT v.* FROM visitrequests v
                   LEFT JOIN blacklist b ON v.visitor_passportdata = b.passportdata
                   WHERE v.status = 'одобрена' AND b.passportdata IS NULL";

            if (!string.IsNullOrEmpty(date))
                sql += " AND v.startdate = @date";
            if (!string.IsNullOrEmpty(type) && type != "Все")
                sql += " AND v.requesttype = @type";
            if (!string.IsNullOrEmpty(department) && department != "Все")
                sql += " AND v.targetdepartment = @dept";
            if (!string.IsNullOrEmpty(searchText))
                sql += @" AND (v.visitor_lastname ILIKE @search 
                 OR v.visitor_firstname ILIKE @search 
                 OR v.visitor_patronymic ILIKE @search 
                 OR v.visitor_passportdata ILIKE @search)";

            sql += " ORDER BY v.startdate ASC";

            using var conn = new NpgsqlConnection(connString);
            using var cmd = new NpgsqlCommand(sql, conn);
            if (!string.IsNullOrEmpty(date))
                cmd.Parameters.AddWithValue("date", DateTime.Parse(date));
            if (!string.IsNullOrEmpty(type) && type != "Все")
                cmd.Parameters.AddWithValue("type", type);
            if (!string.IsNullOrEmpty(department) && department != "Все")
                cmd.Parameters.AddWithValue("dept", department);
            if (!string.IsNullOrEmpty(searchText))
                cmd.Parameters.AddWithValue("search", $"%{searchText}%");

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new VisitRequest
                {
                    RequestID = reader.GetInt32(reader.GetOrdinal("requestid")),
                    RequestType = reader["requesttype"]?.ToString() ?? "",
                    Status = reader["status"]?.ToString() ?? "",
                    StartDate = ConvertToDateTime(reader["startdate"]),
                    EndDate = ConvertToDateTime(reader["enddate"]),
                    VisitPurpose = reader["visitpurpose"]?.ToString() ?? "",
                    TargetDepartment = reader["targetdepartment"]?.ToString() ?? "",
                    VisitorLastName = reader["visitor_lastname"]?.ToString() ?? "",
                    VisitorFirstName = reader["visitor_firstname"]?.ToString() ?? "",
                    VisitorPatronymic = reader["visitor_patronymic"]?.ToString() ?? "",
                    VisitorPassportData = reader["visitor_passportdata"]?.ToString() ?? "",
                    VisitorPhone = reader["visitor_phone"]?.ToString() ?? "",
                    ActualEntryTime = reader["actual_entry_time"] == DBNull.Value ? null : ConvertToDateTime(reader["actual_entry_time"]),
                    ActualExitTime = reader["actual_exit_time"] == DBNull.Value ? null : ConvertToDateTime(reader["actual_exit_time"])
                });
            }
            return list;
        }

        public int SetEntryTime(int requestId, DateTime entryTime)
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            string sql = "UPDATE visitrequests SET actual_entry_time = @time WHERE requestid = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("time", entryTime);
            cmd.Parameters.AddWithValue("id", requestId);
            return cmd.ExecuteNonQuery();
        }

        public int SetExitTime(int requestId, DateTime exitTime)
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            string sql = "UPDATE visitrequests SET actual_exit_time = @time WHERE requestid = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("time", exitTime);
            cmd.Parameters.AddWithValue("id", requestId);
            return cmd.ExecuteNonQuery();
        }

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

        // Проверка черного списка
        public bool IsInBlacklist(string passportData)
        {
            var dt = Query("SELECT COUNT(*) FROM blacklist WHERE passportdata = @pd", new NpgsqlParameter("pd", passportData));
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }
    }
}