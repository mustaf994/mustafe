using ReExamManagementSystem.Domain.Common;

namespace ReExamManagementSystem.Domain.Entities;

public class AuditLog : BaseEntity
{
    /// <summary>Identity user id of the actor. Null for unauthenticated events (e.g. failed login).</summary>
    public string? UserId { get; set; }
    public string? UserName { get; set; }

    /// <summary>e.g. "Login", "Create", "Update", "Delete", "Approve", "Reject", "Publish".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>e.g. "Student", "ReExamApplication".</summary>
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }

    public string? IpAddress { get; set; }
    public string? Description { get; set; }
}
