using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class ExamScheduleListItemViewModel
{
    public int Id { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string RoomLabel { get; set; } = string.Empty;
    public int RoomCapacity { get; set; }
    public string InvigilatorName { get; set; } = string.Empty;
    public int RegisteredStudentCount { get; set; }
    public ExamStatus Status { get; set; }
}
