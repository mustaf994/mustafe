using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Student;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class StudentExamScheduleService : IStudentExamScheduleService
{
    private readonly IUnitOfWork _unitOfWork;

    public StudentExamScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    private async Task<List<ExamSchedule>> GetMyScheduledExamEntitiesAsync(int studentId, CancellationToken cancellationToken)
    {
        var approvedCourseKeys = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Where(a => a.StudentId == studentId && a.Status == ApplicationStatus.Approved)
            .Select(a => new { a.CourseId, a.AcademicYearId, a.SemesterId })
            .ToListAsync(cancellationToken);

        if (approvedCourseKeys.Count == 0) return new List<ExamSchedule>();

        var courseIds = approvedCourseKeys.Select(k => k.CourseId).Distinct().ToList();

        var candidateSchedules = await _unitOfWork.Repository<ExamSchedule>().Query()
            .Include(s => s.Course)
            .Include(s => s.ExamRoom)
            .Where(s => courseIds.Contains(s.CourseId))
            .ToListAsync(cancellationToken);

        // Exact (course, term) match done client-side: EF Core cannot reliably
        // translate an .Any() over a client-side list of composite keys.
        return candidateSchedules
            .Where(s => approvedCourseKeys.Any(k => k.CourseId == s.CourseId && k.AcademicYearId == s.AcademicYearId && k.SemesterId == s.SemesterId))
            .ToList();
    }

    public async Task<IReadOnlyList<ScheduledExamViewModel>> GetMyScheduledExamsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var schedules = await GetMyScheduledExamEntitiesAsync(studentId, cancellationToken);

        return schedules
            .OrderBy(s => s.ExamDate).ThenBy(s => s.StartTime)
            .Select(s => new ScheduledExamViewModel
            {
                ScheduleId = s.Id,
                CourseCode = s.Course.Code,
                CourseName = s.Course.Name,
                ExamDate = s.ExamDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                RoomLabel = s.ExamRoom.Building + " " + s.ExamRoom.RoomNumber
            })
            .ToList();
    }

    public async Task<ExamSlipViewModel?> GetExamSlipAsync(int studentId, int scheduleId, CancellationToken cancellationToken = default)
    {
        var student = await _unitOfWork.Repository<Student>().Query()
            .Include(s => s.Department)
            .Include(s => s.Program)
            .SingleOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student is null) return null;

        var schedules = await GetMyScheduledExamEntitiesAsync(studentId, cancellationToken);
        var schedule = schedules.SingleOrDefault(s => s.Id == scheduleId);
        if (schedule is null) return null;

        return new ExamSlipViewModel
        {
            ScheduleId = schedule.Id,
            ReferenceNumber = $"SLIP-{scheduleId:D6}-{studentId:D6}",
            StudentNumber = student.StudentNumber,
            StudentName = student.FullName,
            DepartmentName = student.Department.Name,
            ProgramName = student.Program.Name,
            CourseCode = schedule.Course.Code,
            CourseName = schedule.Course.Name,
            ExamDate = schedule.ExamDate,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            RoomLabel = $"{schedule.ExamRoom.Building} {schedule.ExamRoom.RoomNumber}"
        };
    }
}
