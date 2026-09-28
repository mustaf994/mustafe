using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IExamRoomService
{
    Task<PagedResult<ExamRoomListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ExamRoomFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(ExamRoomFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(ExamRoomFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
