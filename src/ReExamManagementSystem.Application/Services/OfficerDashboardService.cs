using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class OfficerDashboardService : IOfficerDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public OfficerDashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OfficerDashboardViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var applications = _unitOfWork.Repository<ReExamApplication>().Query();
        var schedules = _unitOfWork.Repository<ExamSchedule>().Query();
        var results = _unitOfWork.Repository<ReExamResult>().Query();

        var model = new OfficerDashboardViewModel
        {
            EligibleStudents = await _unitOfWork.Repository<ReExamEligibility>().Query()
                .Where(e => e.Status == EligibilityStatus.Eligible)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync(cancellationToken),
            PendingApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Pending, cancellationToken),
            ApprovedApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Approved, cancellationToken),
            RejectedApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Rejected, cancellationToken),
            UpcomingExams = await schedules.CountAsync(s => s.Status == ExamStatus.Scheduled && s.ExamDate >= today, cancellationToken),
            CompletedExams = await schedules.CountAsync(s => s.Status == ExamStatus.Completed, cancellationToken),
            PendingResults = await results.CountAsync(r => !r.IsPublished, cancellationToken),
            PublishedResults = await results.CountAsync(r => r.IsPublished, cancellationToken)
        };

        model.RecentApplications = await applications
            .Include(a => a.Student)
            .Include(a => a.Course)
            .OrderByDescending(a => a.ApplicationDate)
            .Take(8)
            .Select(a => new ApplicationListItemViewModel
            {
                Id = a.Id,
                StudentNumber = a.Student.StudentNumber,
                StudentName = a.Student.FullName,
                CourseCode = a.Course.Code,
                CourseName = a.Course.Name,
                ApplicationDate = a.ApplicationDate,
                Status = a.Status
            })
            .ToListAsync(cancellationToken);

        model.ApplicationsByStatus = Enum.GetValues<ApplicationStatus>()
            .Select(status => new ChartPoint { Label = status.ToString(), Value = 0 })
            .ToList();

        var counts = await applications
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        foreach (var point in model.ApplicationsByStatus)
        {
            var match = counts.FirstOrDefault(c => c.Status.ToString() == point.Label);
            if (match is not null) point.Value = match.Count;
        }
        model.ApplicationsByStatus = model.ApplicationsByStatus.Where(p => p.Value > 0).ToList();

        return model;
    }
}
