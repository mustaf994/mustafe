namespace ReExamManagementSystem.Application.ViewModels.Student;

public class EligibleCourseViewModel
{
    public int EligibilityId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal PreviousMark { get; set; }
    public string PreviousGrade { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool IsOpenForReExam { get; set; }
}
