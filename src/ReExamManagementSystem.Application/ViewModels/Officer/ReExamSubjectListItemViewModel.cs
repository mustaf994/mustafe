namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class ReExamSubjectListItemViewModel
{
    public int Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public int RegisteredStudentCount { get; set; }
    public bool IsActive { get; set; }
}
