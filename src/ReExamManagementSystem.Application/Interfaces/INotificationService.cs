using ReExamManagementSystem.Application.ViewModels.Shared;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.Interfaces;

public interface INotificationService
{
    Task CreateAsync(string userId, string title, string message, NotificationType type, string? link = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationViewModel>> GetRecentAsync(string userId, int count = 8, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationViewModel>> GetAllAsync(string userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
}
