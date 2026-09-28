using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Officer;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IResultService
{
    /// <summary>Approved applications that don't have a result recorded yet.</summary>
    Task<PagedResult<PendingResultEntryViewModel>> GetPendingEntryAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResultEntryFormViewModel?> GetEntryFormAsync(int applicationId, CancellationToken cancellationToken = default);
    Task<ServiceResult> EnterMarksAsync(int applicationId, decimal reExamMark, CancellationToken cancellationToken = default);

    Task<PagedResult<ResultListItemViewModel>> GetPagedAsync(bool? verifiedFilter, bool? publishedFilter, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ResultDetailsViewModel?> GetDetailsAsync(int resultId, CancellationToken cancellationToken = default);
    Task<ServiceResult> VerifyAsync(int resultId, string verifierUserId, CancellationToken cancellationToken = default);
    Task<ServiceResult> PublishAsync(int resultId, string publisherUserId, CancellationToken cancellationToken = default);
}
