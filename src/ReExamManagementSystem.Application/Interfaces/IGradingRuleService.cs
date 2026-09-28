using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IGradingRuleService
{
    Task<IReadOnlyList<GradingRuleFormViewModel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GradingRuleFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(GradingRuleFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(GradingRuleFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
