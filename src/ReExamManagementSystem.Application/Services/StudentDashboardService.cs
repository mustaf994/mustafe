using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Application.ViewModels.Student;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class StudentDashboardService : IStudentDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IStudentExamScheduleService _examScheduleService;
    private readonly IStudentResultService _resultService;
    private readonly INotificationService _notificationService;

    public StudentDashboardService(
        IUnitOfWork unitOfWork,
        IStudentExamScheduleService examScheduleService,
        IStudentResultService resultService,
        INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _examScheduleService = examScheduleService;
        _resultService = resultService;
        _notificationService = notificationService;
    }

    public async Task<StudentDashboardViewModel> GetAsync(int studentId, string userId, CancellationToken cancellationToken = default)
    {
        var applications = _unitOfWork.Repository<ReExamApplication>().Query().Where(a => a.StudentId == studentId);

        var model = new StudentDashboardViewModel
        {
            EligibleSubjects = await _unitOfWork.Repository<ReExamEligibility>().Query()
                .CountAsync(e => e.StudentId == studentId && e.Status == EligibilityStatus.Eligible, cancellationToken),
            TotalApplications = await applications.CountAsync(cancellationToken),
            PendingApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Pending, cancellationToken),
            ApprovedApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Approved, cancellationToken),
            PublishedResultsCount = await _unitOfWork.Repository<ReExamResult>().Query()
                .CountAsync(r => r.StudentId == studentId && r.IsPublished, cancellationToken)
        };

        var upcomingExams = await _examScheduleService.GetMyScheduledExamsAsync(studentId, cancellationToken);
        model.UpcomingExams = upcomingExams.Take(5).ToList();
        model.UpcomingExamsCount = upcomingExams.Count;

        var publishedResults = await _resultService.GetPublishedResultsAsync(studentId, cancellationToken);
        model.RecentResults = publishedResults.Take(5).ToList();

        model.RecentNotifications = (await _notificationService.GetRecentAsync(userId, 5, cancellationToken)).ToList();

        var counts = await applications
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        model.ApplicationsByStatus = counts
            .Select(c => new ChartPoint { Label = c.Status.ToString(), Value = c.Count })
            .ToList();

        return model;
    }
}
