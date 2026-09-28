using ReExamManagementSystem.Domain.Common;

namespace ReExamManagementSystem.Domain.Entities;

/// <summary>e.g. "Semester One".</summary>
public class Semester : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;
}
