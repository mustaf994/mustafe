using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Officer;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IReExamSubjectService
{
    Task<PagedResult<ReExamSubjectListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ReExamSubjectFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default);
    Task<ReExamSubjectFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(ReExamSubjectFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(ReExamSubjectFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
