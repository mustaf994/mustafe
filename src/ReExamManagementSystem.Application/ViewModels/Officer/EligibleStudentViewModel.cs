namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class EligibleStudentViewModel
{
    public int EligibilityId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal PreviousMark { get; set; }
    public string PreviousGrade { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool HasActiveApplication { get; set; }
}
