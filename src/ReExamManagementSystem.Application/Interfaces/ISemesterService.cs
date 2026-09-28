using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface ISemesterService
{
    Task<PagedResult<SemesterListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<SemesterFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default);
    Task<SemesterFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(SemesterFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(SemesterFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
