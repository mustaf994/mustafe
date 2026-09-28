using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface ISystemSettingService
{
    Task<IReadOnlyList<SystemSettingFormViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<SystemSettingFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(SystemSettingFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(SystemSettingFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
