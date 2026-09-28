using Microsoft.AspNetCore.Identity;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Infrastructure.Services;

public class StaffUserService : IStaffUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserAccountService _userAccountService;

    public StaffUserService(UserManager<ApplicationUser> userManager, IUserAccountService userAccountService)
    {
        _userManager = userManager;
        _userAccountService = userAccountService;
    }

    public async Task<IReadOnlyList<StaffUserListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var admins = await _userManager.GetUsersInRoleAsync(Roles.Administrator);
        var officers = await _userManager.GetUsersInRoleAsync(Roles.ExaminationOfficer);

        var results = new List<StaffUserListItemViewModel>();
        results.AddRange(admins.Select(u => ToViewModel(u, Roles.Administrator)));
        results.AddRange(officers.Select(u => ToViewModel(u, Roles.ExaminationOfficer)));

        return results.OrderBy(u => u.FullName).ToList();
    }

    public async Task<ServiceResult<string>> CreateAsync(StaffUserFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Role != Roles.Administrator && model.Role != Roles.ExaminationOfficer)
        {
            return ServiceResult<string>.Failure("Invalid role.");
        }

        var result = await _userAccountService.CreateUserAsync(model.Email, model.FullName, model.Role, cancellationToken);
        if (!result.Succeeded)
        {
            return ServiceResult<string>.Failure(result.Errors.ToArray());
        }

        return ServiceResult<string>.Success(result.Data.TemporaryPassword);
    }

    public Task<ServiceResult> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken = default) =>
        _userAccountService.SetActiveAsync(userId, isActive, cancellationToken);

    public Task<ServiceResult> DeleteAsync(string userId, CancellationToken cancellationToken = default) =>
        _userAccountService.DeleteUserAsync(userId, cancellationToken);

    private static StaffUserListItemViewModel ToViewModel(ApplicationUser user, string role) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email ?? string.Empty,
        Role = role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}
