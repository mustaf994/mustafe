using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class ExamAttendanceService : IExamAttendanceService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExamAttendanceService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AttendanceViewModel?> GetForScheduleAsync(int scheduleId, CancellationToken cancellationToken = default)
    {
        var schedule = await _unitOfWork.Repository<ExamSchedule>().Query()
            .Include(s => s.Course)
            .SingleOrDefaultAsync(s => s.Id == scheduleId, cancellationToken);

        if (schedule is null) return null;

        var registeredStudents = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Where(a =>
                a.CourseId == schedule.CourseId &&
                a.AcademicYearId == schedule.AcademicYearId &&
                a.SemesterId == schedule.SemesterId &&
                a.Status == ApplicationStatus.Approved)
            .Select(a => a.Student)
            .Distinct()
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

        var existingAttendance = await _unitOfWork.Repository<ExamAttendance>()
            .FindAsync(a => a.ExamScheduleId == scheduleId, cancellationToken);
        var attendanceByStudent = existingAttendance.ToDictionary(a => a.StudentId, a => a.Status);

        return new AttendanceViewModel
        {
            ScheduleId = schedule.Id,
            CourseCode = schedule.Course.Code,
            CourseName = schedule.Course.Name,
            ExamDate = schedule.ExamDate,
            Students = registeredStudents.Select(s => new StudentAttendanceRowViewModel
            {
                StudentId = s.Id,
                StudentNumber = s.StudentNumber,
                StudentName = s.FullName,
                Status = attendanceByStudent.TryGetValue(s.Id, out var status) ? status : null
            }).ToList()
        };
    }

    public async Task<ServiceResult> SaveAsync(int scheduleId, IReadOnlyDictionary<int, AttendanceStatus> attendance, string recordedByUserId, CancellationToken cancellationToken = default)
    {
        var schedule = await _unitOfWork.Repository<ExamSchedule>().GetByIdAsync(scheduleId, cancellationToken);
        if (schedule is null) return ServiceResult.Failure("Exam schedule not found.");

        var existing = await _unitOfWork.Repository<ExamAttendance>()
            .FindAsync(a => a.ExamScheduleId == scheduleId, cancellationToken);
        var existingByStudent = existing.ToDictionary(a => a.StudentId);

        var now = DateTime.UtcNow;

        foreach (var (studentId, status) in attendance)
        {
            if (existingByStudent.TryGetValue(studentId, out var record))
            {
                record.Status = status;
                record.RecordedAt = now;
                record.RecordedByUserId = recordedByUserId;
                _unitOfWork.Repository<ExamAttendance>().Update(record);
            }
            else
            {
                await _unitOfWork.Repository<ExamAttendance>().AddAsync(new ExamAttendance
                {
                    ExamScheduleId = scheduleId,
                    StudentId = studentId,
                    Status = status,
                    RecordedAt = now,
                    RecordedByUserId = recordedByUserId
                }, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
