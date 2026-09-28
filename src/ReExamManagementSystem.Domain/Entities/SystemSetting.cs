using ReExamManagementSystem.Domain.Common;

namespace ReExamManagementSystem.Domain.Entities;

/// <summary>
/// Generic key/value store for configurable business rules (e.g. re-exam
/// final-mark calculation policy, maximum applications per student) so they
/// never end up hard-coded in a service.
/// </summary>
public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Description { get; set; }
}
