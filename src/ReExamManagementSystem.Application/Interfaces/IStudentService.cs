using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IStudentService
{
    Task<PagedResult<StudentListItemViewModel>> GetPagedAsync(string? search, int? departmentId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int?> GetStudentIdByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<StudentFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default);
    Task<StudentFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<StudentDetailsViewModel?> GetDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<string>> CreateAsync(StudentFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAsync(StudentFormViewModel model, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
