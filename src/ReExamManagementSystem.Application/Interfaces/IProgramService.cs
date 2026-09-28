using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IProgramService
{
    Task<PagedResult<ProgramListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ProgramFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default);
    Task<ProgramFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(ProgramFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(ProgramFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
