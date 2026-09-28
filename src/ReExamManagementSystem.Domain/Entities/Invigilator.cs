using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

public class Invigilator : BaseEntity
{
    public string StaffId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public InvigilatorStatus Status { get; set; } = InvigilatorStatus.Active;

    /// <summary>Optional link to an Identity user, if this invigilator also has a system login.</summary>
    public string? UserId { get; set; }

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();
}
