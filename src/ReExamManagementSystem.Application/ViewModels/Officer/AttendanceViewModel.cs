using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class AttendanceViewModel
{
    public int ScheduleId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public DateTime ExamDate { get; set; }
    public List<StudentAttendanceRowViewModel> Students { get; set; } = new();
}

public class StudentAttendanceRowViewModel
{
    public int StudentId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public AttendanceStatus? Status { get; set; }
}
