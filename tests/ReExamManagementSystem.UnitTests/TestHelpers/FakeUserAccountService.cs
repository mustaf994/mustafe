using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;

namespace ReExamManagementSystem.UnitTests.TestHelpers;

/// <summary>No-op stand-in for the real Identity-backed IUserAccountService, so Application-layer services can be unit tested without a real ASP.NET Core Identity store.</summary>
public class FakeUserAccountService : IUserAccountService
{
    public List<string> OfficerIds { get; set; } = new();

    public Task<ServiceResult<(string UserId, string TemporaryPassword)>> CreateUserAsync(string email, string fullName, string role, CancellationToken cancellationToken = default)
        => Task.FromResult(ServiceResult<(string, string)>.Success((Guid.NewGuid().ToString(), "Temp123!")));

    public Task<ServiceResult> UpdateProfileAsync(string userId, string fullName, string? phoneNumber, CancellationToken cancellationToken = default)
        => Task.FromResult(ServiceResult.Success());

    public Task<ServiceResult> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken = default)
        => Task.FromResult(ServiceResult.Success());

    public Task<ServiceResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        => Task.FromResult(ServiceResult.Success());

    public Task<IReadOnlyList<string>> GetUserIdsInRoleAsync(string role, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<string>>(OfficerIds);
}
