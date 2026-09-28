namespace ReExamManagementSystem.Domain.Common;

/// <summary>
/// Base class for all domain entities providing an integer surrogate key
/// and audit timestamps required across the system's tables.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
