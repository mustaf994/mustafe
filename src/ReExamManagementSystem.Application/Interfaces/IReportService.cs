using ReExamManagementSystem.Application.ViewModels.Shared;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IReportService
{
    Task<ReportData> GetStudentListReportAsync(int? departmentId, StudentStatus? status, CancellationToken cancellationToken = default);
    Task<ReportData> GetEligibleStudentReportAsync(CancellationToken cancellationToken = default);
    Task<ReportData> GetApplicationReportAsync(ApplicationStatus? status, CancellationToken cancellationToken = default);
    Task<ReportData> GetResultReportAsync(bool? published, CancellationToken cancellationToken = default);
    Task<ReportData> GetExaminationTimetableReportAsync(CancellationToken cancellationToken = default);
}
