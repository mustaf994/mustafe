using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

public class Notification : BaseEntity
{
    /// <summary>Identity user id of the recipient.</summary>
    public string UserId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }

    /// <summary>Relative URL the notification should link to, if any.</summary>
    public string? Link { get; set; }
}
