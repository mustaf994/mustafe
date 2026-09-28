using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

public class ExamAttendance : BaseEntity
{
    public int ExamScheduleId { get; set; }
    public ExamSchedule ExamSchedule { get; set; } = null!;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public AttendanceStatus Status { get; set; }
    public DateTime? RecordedAt { get; set; }

    /// <summary>Identity user id of the Examination Officer who recorded attendance.</summary>
    public string? RecordedByUserId { get; set; }
}
