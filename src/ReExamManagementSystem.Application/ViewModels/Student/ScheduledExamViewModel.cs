namespace ReExamManagementSystem.Application.ViewModels.Student;

public class ScheduledExamViewModel
{
    public int ScheduleId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string RoomLabel { get; set; } = string.Empty;
}

public class ExamSlipViewModel
{
    public int ScheduleId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string RoomLabel { get; set; } = string.Empty;
}
