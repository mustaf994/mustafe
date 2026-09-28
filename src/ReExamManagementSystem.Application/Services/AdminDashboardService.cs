using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public AdminDashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AdminDashboardViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var sixMonthsAgo = today.AddMonths(-5);

        var applications = _unitOfWork.Repository<ReExamApplication>().Query();

        var model = new AdminDashboardViewModel
        {
            TotalStudents = await _unitOfWork.Repository<Student>().Query().CountAsync(cancellationToken),
            TotalDepartments = await _unitOfWork.Repository<Department>().Query().CountAsync(cancellationToken),
            TotalPrograms = await _unitOfWork.Repository<AcademicProgram>().Query().CountAsync(cancellationToken),
            TotalCourses = await _unitOfWork.Repository<Course>().Query().CountAsync(cancellationToken),
            EligibleStudents = await _unitOfWork.Repository<ReExamEligibility>().Query()
                .Where(e => e.Status == EligibilityStatus.Eligible)
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync(cancellationToken),
            TotalApplications = await applications.CountAsync(cancellationToken),
            PendingApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Pending, cancellationToken),
            ApprovedApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Approved, cancellationToken),
            RejectedApplications = await applications.CountAsync(a => a.Status == ApplicationStatus.Rejected, cancellationToken),
            UpcomingExams = await _unitOfWork.Repository<ExamSchedule>().Query()
                .CountAsync(s => s.Status == ExamStatus.Scheduled && s.ExamDate >= today, cancellationToken),
            PublishedResults = await _unitOfWork.Repository<ReExamResult>().Query()
                .CountAsync(r => r.IsPublished, cancellationToken)
        };

        var recentApplicationsRaw = await applications
            .Include(a => a.Student)
            .Include(a => a.Course)
            .OrderByDescending(a => a.ApplicationDate)
            .Take(8)
            .Select(a => new RecentApplicationViewModel
            {
                Id = a.Id,
                StudentName = a.Student.FullName,
                CourseCode = a.Course.Code,
                Status = a.Status.ToString(),
                ApplicationDate = a.ApplicationDate
            })
            .ToListAsync(cancellationToken);
        model.RecentApplications = recentApplicationsRaw;

        var applicationsInRange = await applications
            .Where(a => a.ApplicationDate >= sixMonthsAgo)
            .Select(a => a.ApplicationDate)
            .ToListAsync(cancellationToken);

        model.ApplicationsByMonth = Enumerable.Range(0, 6)
            .Select(offset => sixMonthsAgo.AddMonths(offset))
            .Select(month => new ChartPoint
            {
                Label = month.ToString("MMM yyyy"),
                Value = applicationsInRange.Count(d => d.Year == month.Year && d.Month == month.Month)
            })
            .ToList();

        model.StudentsByDepartment = await _unitOfWork.Repository<Student>().Query()
            .Include(s => s.Department)
            .GroupBy(s => s.Department.Name)
            .Select(g => new ChartPoint { Label = g.Key, Value = g.Count() })
            .OrderByDescending(p => p.Value)
            .ToListAsync(cancellationToken);

        return model;
    }
}
