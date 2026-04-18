using System;

namespace HranitelProSecurity
{
    public class User
    {
        public int EmployeeID { get; set; }
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string Patronymic { get; set; } = "";
        public string EmployeeCode { get; set; } = "";
        public string Department { get; set; } = "";
        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
    }

    public class VisitRequest
    {
        public int RequestID { get; set; }
        public string RequestType { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string VisitPurpose { get; set; } = "";
        public string TargetDepartment { get; set; } = "";
        public string VisitorLastName { get; set; } = "";
        public string VisitorFirstName { get; set; } = "";
        public string VisitorPatronymic { get; set; } = "";
        public string VisitorPassportData { get; set; } = "";
        public string VisitorPhone { get; set; } = "";
        public DateTime? ActualEntryTime { get; set; }
        public DateTime? ActualExitTime { get; set; }
        public string FullName => $"{VisitorLastName} {VisitorFirstName} {VisitorPatronymic}".Trim();
    }
}