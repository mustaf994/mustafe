using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

public class ExamSchedule : BaseEntity
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public int SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    public DateTime ExamDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int ExamRoomId { get; set; }
    public ExamRoom ExamRoom { get; set; } = null!;

    public int InvigilatorId { get; set; }
    public Invigilator Invigilator { get; set; } = null!;

    public ExamStatus Status { get; set; } = ExamStatus.Scheduled;

    public ICollection<ExamAttendance> Attendances { get; set; } = new List<ExamAttendance>();
}
