using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Officer;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IEligibilityService
{
    Task<PagedResult<EligibleStudentViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Scans every StudentResult with a failing status that does not yet have a
    /// ReExamEligibility record and creates one. Idempotent - safe to run
    /// repeatedly as new results are recorded. Returns how many new eligibility
    /// records were created.
    /// </summary>
    Task<int> RecomputeAsync(CancellationToken cancellationToken = default);
}
