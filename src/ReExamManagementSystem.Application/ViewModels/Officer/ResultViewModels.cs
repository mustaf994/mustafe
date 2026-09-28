using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class PendingResultEntryViewModel
{
    public int ApplicationId { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal OriginalMark { get; set; }
    public string OriginalGrade { get; set; } = string.Empty;
}

public class ResultEntryFormViewModel
{
    public int ApplicationId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal OriginalMark { get; set; }
    public string OriginalGrade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Re-exam mark is required.")]
    [Range(0, 100, ErrorMessage = "Mark must be between 0 and 100.")]
    [Display(Name = "Re-Exam Mark")]
    public decimal ReExamMark { get; set; }
}

public class ResultListItemViewModel
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public decimal? OriginalMark { get; set; }
    public decimal? ReExamMark { get; set; }
    public decimal FinalMark { get; set; }
    public string Grade { get; set; } = string.Empty;
    public ResultStatus Status { get; set; }
    public bool IsVerified { get; set; }
    public bool IsPublished { get; set; }
}

public class ResultDetailsViewModel
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal? OriginalMark { get; set; }
    public decimal? ReExamMark { get; set; }
    public decimal FinalMark { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public ResultStatus Status { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
}
