using ReExamManagementSystem.Application.Common;

namespace ReExamManagementSystem.Application.Interfaces;

/// <summary>
/// Abstracts ASP.NET Core Identity user management behind an Application-layer
/// interface. Domain/Application services that need to provision a login
/// (e.g. creating a Student or Invigilator account) depend on this instead of
/// referencing Infrastructure's ApplicationUser/UserManager directly.
/// </summary>
public interface IUserAccountService
{
    /// <summary>Creates a login with a system-generated temporary password and assigns the given role. Returns the new Identity user id and the temporary password (shown once to the administrator).</summary>
    Task<ServiceResult<(string UserId, string TemporaryPassword)>> CreateUserAsync(
        string email, string fullName, string role, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateProfileAsync(string userId, string fullName, string? phoneNumber, CancellationToken cancellationToken = default);
    Task<ServiceResult> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteUserAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>Ids of every user currently in the given role, e.g. to broadcast a notification to all Examination Officers.</summary>
    Task<IReadOnlyList<string>> GetUserIdsInRoleAsync(string role, CancellationToken cancellationToken = default);
}
