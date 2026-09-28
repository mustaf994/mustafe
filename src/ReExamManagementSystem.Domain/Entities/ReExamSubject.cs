using ReExamManagementSystem.Domain.Common;

namespace ReExamManagementSystem.Domain.Entities;

/// <summary>
/// Marks a Course as open for re-examination in a given academic year/semester.
/// Distinct from the Course catalog entry itself, since not every course is
/// offered for re-exam every term.
/// </summary>
public class ReExamSubject : BaseEntity
{
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public int SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
