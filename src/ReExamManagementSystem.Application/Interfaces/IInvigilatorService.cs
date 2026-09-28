using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IInvigilatorService
{
    Task<PagedResult<InvigilatorListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<InvigilatorFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default);
    Task<InvigilatorFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(InvigilatorFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(InvigilatorFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
