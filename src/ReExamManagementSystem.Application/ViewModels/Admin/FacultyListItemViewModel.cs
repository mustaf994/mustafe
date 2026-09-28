namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class FacultyListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DepartmentCount { get; set; }
}
