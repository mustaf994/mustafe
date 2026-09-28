using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Application.ViewModels.Student;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IReExamApplicationService
{
    // Student-facing
    Task<IReadOnlyList<EligibleCourseViewModel>> GetEligibleCoursesForStudentAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ServiceResult> SubmitAsync(int studentId, int eligibilityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MyApplicationViewModel>> GetMyApplicationsAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ServiceResult> CancelAsync(int studentId, int applicationId, CancellationToken cancellationToken = default);

    // Officer-facing
    Task<PagedResult<ApplicationListItemViewModel>> GetPagedForReviewAsync(ApplicationStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ApplicationDetailsViewModel?> GetReviewDetailsAsync(int applicationId, CancellationToken cancellationToken = default);
    Task<ServiceResult> ApproveAsync(int applicationId, string officerUserId, CancellationToken cancellationToken = default);
    Task<ServiceResult> RejectAsync(int applicationId, string officerUserId, string reason, CancellationToken cancellationToken = default);
}
