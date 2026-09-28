namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class DepartmentListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string FacultyName { get; set; } = string.Empty;
    public int ProgramCount { get; set; }
    public int CourseCount { get; set; }
    public int StudentCount { get; set; }
}
