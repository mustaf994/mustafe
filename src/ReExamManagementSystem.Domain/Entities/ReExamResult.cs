using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

/// <summary>
/// Holds both the original and the re-examination outcome for one approved
/// application, plus the final mark/grade computed by the configurable
/// re-exam result rule, and the verification/publication workflow state.
/// </summary>
public class ReExamResult : BaseEntity
{
    public int ReExamApplicationId { get; set; }
    public ReExamApplication ReExamApplication { get; set; } = null!;

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public decimal? OriginalMark { get; set; }
    public decimal? ReExamMark { get; set; }
    public decimal FinalMark { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public ResultStatus Status { get; set; }

    public bool IsVerified { get; set; }
    public string? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public bool IsPublished { get; set; }
    public string? PublishedByUserId { get; set; }
    public DateTime? PublishedAt { get; set; }
}
