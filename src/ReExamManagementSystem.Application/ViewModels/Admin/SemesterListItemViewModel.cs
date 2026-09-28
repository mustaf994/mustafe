namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class SemesterListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
}
