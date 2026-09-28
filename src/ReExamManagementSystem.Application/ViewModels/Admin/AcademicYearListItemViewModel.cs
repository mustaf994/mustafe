namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class AcademicYearListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int SemesterCount { get; set; }
}
