using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class ApplicationListItemViewModel
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public ApplicationStatus Status { get; set; }
}

public class ApplicationDetailsViewModel
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal PreviousMark { get; set; }
    public string PreviousGrade { get; set; } = string.Empty;
    public string EligibilityReason { get; set; } = string.Empty;
    public DateTime ApplicationDate { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAt { get; set; }
}
