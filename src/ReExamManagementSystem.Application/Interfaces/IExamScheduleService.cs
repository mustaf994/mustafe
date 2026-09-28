using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Officer;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IExamScheduleService
{
    Task<PagedResult<ExamScheduleListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ExamScheduleFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default);
    Task<ExamScheduleFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(ExamScheduleFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(ExamScheduleFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
