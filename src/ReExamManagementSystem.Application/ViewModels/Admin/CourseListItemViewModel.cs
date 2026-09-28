using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class CourseListItemViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public int Level { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public CourseStatus Status { get; set; }
}
