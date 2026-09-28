using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class ProgramListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DurationYears { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public ProgramStatus Status { get; set; }
    public int StudentCount { get; set; }
}
