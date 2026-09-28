using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface ICourseService
{
    Task<PagedResult<CourseListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<CourseFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default);
    Task<CourseFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> CreateAsync(CourseFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(CourseFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
