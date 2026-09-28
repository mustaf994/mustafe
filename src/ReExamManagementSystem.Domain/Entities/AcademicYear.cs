using ReExamManagementSystem.Domain.Common;

namespace ReExamManagementSystem.Domain.Entities;

/// <summary>e.g. "2025/2026".</summary>
public class AcademicYear : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Semester> Semesters { get; set; } = new List<Semester>();
}
