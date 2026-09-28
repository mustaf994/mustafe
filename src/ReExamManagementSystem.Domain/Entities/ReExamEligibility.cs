using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

/// <summary>
/// The system's automatic determination of whether a student may retake a
/// course, derived from their StudentResult by the eligibility rule engine
/// (Application layer). One row per failed/incomplete result considered.
/// </summary>
public class ReExamEligibility : BaseEntity
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int StudentResultId { get; set; }
    public StudentResult StudentResult { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public int SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    public EligibilityStatus Status { get; set; }
    public string Reason { get; set; } = string.Empty;

    public ICollection<ReExamApplication> Applications { get; set; } = new List<ReExamApplication>();
}
