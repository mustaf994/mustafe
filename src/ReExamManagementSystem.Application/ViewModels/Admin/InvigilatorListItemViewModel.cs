using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class InvigilatorListItemViewModel
{
    public int Id { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public InvigilatorStatus Status { get; set; }
}
