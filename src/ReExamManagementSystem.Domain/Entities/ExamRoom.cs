using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

public class ExamRoom : BaseEntity
{
    public string RoomNumber { get; set; } = string.Empty;
    public string Building { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Location { get; set; }
    public RoomStatus Status { get; set; } = RoomStatus.Available;

    public ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();
}
