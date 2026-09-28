using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

/// <summary>
/// Administrator-only management of Administrator and Examination Officer
/// login accounts. Student accounts are provisioned separately, alongside
/// their academic record, by IStudentService.
/// </summary>
public interface IStaffUserService
{
    Task<IReadOnlyList<StaffUserListItemViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ServiceResult<string>> CreateAsync(StaffUserFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> SetActiveAsync(string userId, bool isActive, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(string userId, CancellationToken cancellationToken = default);
}
