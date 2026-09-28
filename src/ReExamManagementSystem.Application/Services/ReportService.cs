using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Shared;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ReportData> GetStudentListReportAsync(int? departmentId, StudentStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Student>().Query()
            .Include(s => s.Department)
            .Include(s => s.Program)
            .AsQueryable();

        if (departmentId.HasValue) query = query.Where(s => s.DepartmentId == departmentId.Value);
        if (status.HasValue) query = query.Where(s => s.Status == status.Value);

        var students = await query.OrderBy(s => s.FullName).ToListAsync(cancellationToken);

        var headers = new[] { "Student Number", "Name", "Department", "Program", "Level", "Email", "Status" };
        var rows = students.Select(s => (IReadOnlyList<string>)new[]
        {
            s.StudentNumber, s.FullName, s.Department.Name, s.Program.Name, s.Level.ToString(), s.Email, s.Status.ToString()
        }).ToList();

        return new ReportData("Student List Report", headers, rows);
    }

    public async Task<ReportData> GetEligibleStudentReportAsync(CancellationToken cancellationToken = default)
    {
        var eligibilities = await _unitOfWork.Repository<ReExamEligibility>().Query()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.StudentResult)
            .Where(e => e.Status == EligibilityStatus.Eligible)
            .OrderBy(e => e.Student.FullName)
            .ToListAsync(cancellationToken);

        var headers = new[] { "Student Number", "Name", "Course", "Previous Mark", "Previous Grade", "Reason" };
        var rows = eligibilities.Select(e => (IReadOnlyList<string>)new[]
        {
            e.Student.StudentNumber, e.Student.FullName, $"{e.Course.Code} - {e.Course.Name}",
            e.StudentResult.Marks.ToString("0.##"), e.StudentResult.Grade, e.Reason
        }).ToList();

        return new ReportData("Eligible Students Report", headers, rows);
    }

    public async Task<ReportData> GetApplicationReportAsync(ApplicationStatus? status, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.Student)
            .Include(a => a.Course)
            .AsQueryable();

        if (status.HasValue) query = query.Where(a => a.Status == status.Value);

        var applications = await query.OrderByDescending(a => a.ApplicationDate).ToListAsync(cancellationToken);

        var headers = new[] { "Student Number", "Name", "Course", "Applied On", "Status", "Rejection Reason" };
        var rows = applications.Select(a => (IReadOnlyList<string>)new[]
        {
            a.Student.StudentNumber, a.Student.FullName, $"{a.Course.Code} - {a.Course.Name}",
            a.ApplicationDate.ToString("dd MMM yyyy"), a.Status.ToString(), a.RejectionReason ?? "-"
        }).ToList();

        var title = status.HasValue ? $"{status} Applications Report" : "All Applications Report";
        return new ReportData(title, headers, rows);
    }

    public async Task<ReportData> GetResultReportAsync(bool? published, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<ReExamResult>().Query()
            .Include(r => r.Student)
            .Include(r => r.Course)
            .AsQueryable();

        if (published.HasValue) query = query.Where(r => r.IsPublished == published.Value);

        var results = await query.OrderBy(r => r.Student.FullName).ToListAsync(cancellationToken);

        var headers = new[] { "Student Number", "Name", "Course", "Original", "Re-Exam", "Final", "Grade", "Status", "Published" };
        var rows = results.Select(r => (IReadOnlyList<string>)new[]
        {
            r.Student.StudentNumber, r.Student.FullName, $"{r.Course.Code} - {r.Course.Name}",
            r.OriginalMark?.ToString("0.##") ?? "-", r.ReExamMark?.ToString("0.##") ?? "-",
            r.FinalMark.ToString("0.##"), r.Grade, r.Status.ToString(), r.IsPublished ? "Yes" : "No"
        }).ToList();

        return new ReportData("Re-Exam Results Report", headers, rows);
    }

    public async Task<ReportData> GetExaminationTimetableReportAsync(CancellationToken cancellationToken = default)
    {
        var schedules = await _unitOfWork.Repository<ExamSchedule>().Query()
            .Include(s => s.Course)
            .Include(s => s.ExamRoom)
            .Include(s => s.Invigilator)
            .OrderBy(s => s.ExamDate).ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

        var headers = new[] { "Course", "Date", "Time", "Room", "Invigilator", "Status" };
        var rows = schedules.Select(s => (IReadOnlyList<string>)new[]
        {
            $"{s.Course.Code} - {s.Course.Name}", s.ExamDate.ToString("dd MMM yyyy"),
            $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}", $"{s.ExamRoom.Building} {s.ExamRoom.RoomNumber}",
            s.Invigilator.FullName, s.Status.ToString()
        }).ToList();

        return new ReportData("Examination Timetable Report", headers, rows);
    }
}
