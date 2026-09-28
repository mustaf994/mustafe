using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Shared;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.UnitTests.TestHelpers;

/// <summary>Records every notification it was asked to create, so tests can assert on who got notified without a real database-backed Notification table dependency chain.</summary>
public class FakeNotificationService : INotificationService
{
    public List<(string UserId, string Title, string Message, NotificationType Type)> Created { get; } = new();

    public Task CreateAsync(string userId, string title, string message, NotificationType type, string? link = null, CancellationToken cancellationToken = default)
    {
        Created.Add((userId, title, message, type));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<NotificationViewModel>> GetRecentAsync(string userId, int count = 8, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationViewModel>>(Array.Empty<NotificationViewModel>());

    public Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default) => Task.FromResult(0);

    public Task<IReadOnlyList<NotificationViewModel>> GetAllAsync(string userId, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NotificationViewModel>>(Array.Empty<NotificationViewModel>());

    public Task MarkAsReadAsync(int notificationId, string userId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
