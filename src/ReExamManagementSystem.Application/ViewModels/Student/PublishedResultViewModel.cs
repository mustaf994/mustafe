using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Student;

public class PublishedResultViewModel
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal? OriginalMark { get; set; }
    public decimal? ReExamMark { get; set; }
    public decimal FinalMark { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public ResultStatus Status { get; set; }
    public DateTime? PublishedAt { get; set; }
}
