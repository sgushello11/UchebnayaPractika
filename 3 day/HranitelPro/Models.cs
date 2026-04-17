using System;

namespace HranitelPro
{
    public class User
    {
        public int UserID { get; set; }
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string? Patronymic { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public string PassportData { get; set; } = "";
        public string Login { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
    }

    public class RequestItem
    {
        public int Id { get; set; }
        public string Type { get; set; } = "";
        public string Status { get; set; } = "";
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public string Purpose { get; set; } = "";
        public string Department { get; set; } = "";
        public string CreatedAt { get; set; } = "";
    }

    public class RequestFull
    {
        public int RequestID { get; set; }
        public string RequestType { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string VisitPurpose { get; set; } = "";
        public string TargetDepartment { get; set; } = "";
        public string Note { get; set; } = "";

        public string VisitorLastName { get; set; } = "";
        public string VisitorFirstName { get; set; } = "";
        public string? VisitorPatronymic { get; set; }
        public string? VisitorPhone { get; set; }
        public string VisitorEmail { get; set; } = "";
        public string? VisitorOrganization { get; set; }
        public DateTime VisitorBirthDate { get; set; }
        public string VisitorPassportData { get; set; } = "";
    }

    public class Visitor
    {
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string? Patronymic { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = "";
        public string? Organization { get; set; }
        public DateTime BirthDate { get; set; }
        public string PassportData { get; set; } = "";
    }

    public class AttachedFile
    {
        public int FileId { get; set; }
        public string FileType { get; set; } = "";
        public string FilePath { get; set; } = "";
        public string FileName { get; set; } = "";
    }

    public class GroupMember
    {
        public string LastName { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string? Patronymic { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public string PassportData { get; set; } = "";
    }
}