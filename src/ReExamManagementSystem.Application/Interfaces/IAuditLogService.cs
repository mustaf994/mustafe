namespace ReExamManagementSystem.Application.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(
        string? userId,
        string? userName,
        string action,
        string entityName,
        string? entityId,
        string? ipAddress,
        string? description,
        CancellationToken cancellationToken = default);
}
