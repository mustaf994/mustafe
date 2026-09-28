using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

public class ReExamApplication : BaseEntity
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int ReExamEligibilityId { get; set; }
    public ReExamEligibility ReExamEligibility { get; set; } = null!;

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public int SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    public string? RejectionReason { get; set; }

    /// <summary>Identity user id of the Examination Officer/Administrator who approved or rejected this application.</summary>
    public string? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public ReExamResult? ReExamResult { get; set; }
}
