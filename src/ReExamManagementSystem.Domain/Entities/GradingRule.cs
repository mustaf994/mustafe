using ReExamManagementSystem.Domain.Common;

namespace ReExamManagementSystem.Domain.Entities;

/// <summary>
/// A configurable mark band (e.g. 90-100 = A, GradePoint 4.00). Administrators
/// manage these instead of grade thresholds being hard-coded in the app, per
/// the "grading system must be configurable" requirement.
/// </summary>
public class GradingRule : BaseEntity
{
    public decimal MinMark { get; set; }
    public decimal MaxMark { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public string? Description { get; set; }
    public bool IsPassing { get; set; }
}
