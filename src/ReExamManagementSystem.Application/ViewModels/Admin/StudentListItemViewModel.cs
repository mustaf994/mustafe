using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class StudentListItemViewModel
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public int Level { get; set; }
    public StudentStatus Status { get; set; }
}
