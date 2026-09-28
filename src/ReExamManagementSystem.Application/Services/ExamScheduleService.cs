using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class ExamScheduleService : IExamScheduleService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExamScheduleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ExamScheduleListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<ExamSchedule>().Query()
            .Include(s => s.Course)
            .Include(s => s.ExamRoom)
            .Include(s => s.Invigilator)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Course.Code.Contains(search) || s.Course.Name.Contains(search));
        }

        var applications = _unitOfWork.Repository<ReExamApplication>().Query();

        return await query
            .OrderByDescending(s => s.ExamDate).ThenBy(s => s.StartTime)
            .Select(s => new ExamScheduleListItemViewModel
            {
                Id = s.Id,
                CourseCode = s.Course.Code,
                CourseName = s.Course.Name,
                ExamDate = s.ExamDate,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                RoomLabel = s.ExamRoom.Building + " " + s.ExamRoom.RoomNumber,
                RoomCapacity = s.ExamRoom.Capacity,
                InvigilatorName = s.Invigilator.FullName,
                Status = s.Status,
                RegisteredStudentCount = applications.Count(a =>
                    a.CourseId == s.CourseId &&
                    a.AcademicYearId == s.AcademicYearId &&
                    a.SemesterId == s.SemesterId &&
                    a.Status == ApplicationStatus.Approved)
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<ExamScheduleFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default)
    {
        var model = new ExamScheduleFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<ExamScheduleFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var schedule = await _unitOfWork.Repository<ExamSchedule>().GetByIdAsync(id, cancellationToken);
        if (schedule is null) return null;

        var model = new ExamScheduleFormViewModel
        {
            Id = schedule.Id,
            CourseId = schedule.CourseId,
            AcademicYearId = schedule.AcademicYearId,
            SemesterId = schedule.SemesterId,
            ExamDate = schedule.ExamDate,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            ExamRoomId = schedule.ExamRoomId,
            InvigilatorId = schedule.InvigilatorId,
            Status = schedule.Status
        };
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<ServiceResult> CreateAsync(ExamScheduleFormViewModel model, CancellationToken cancellationToken = default)
    {
        var conflictCheck = await CheckConflictsAsync(model, excludeId: null, cancellationToken);
        if (!conflictCheck.Succeeded) return conflictCheck;

        await _unitOfWork.Repository<ExamSchedule>().AddAsync(new ExamSchedule
        {
            CourseId = model.CourseId,
            AcademicYearId = model.AcademicYearId,
            SemesterId = model.SemesterId,
            ExamDate = model.ExamDate,
            StartTime = model.StartTime,
            EndTime = model.EndTime,
            ExamRoomId = model.ExamRoomId,
            InvigilatorId = model.InvigilatorId,
            Status = model.Status
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(ExamScheduleFormViewModel model, CancellationToken cancellationToken = default)
    {
        var schedule = await _unitOfWork.Repository<ExamSchedule>().GetByIdAsync(model.Id, cancellationToken);
        if (schedule is null) return ServiceResult.Failure("Exam schedule not found.");

        var conflictCheck = await CheckConflictsAsync(model, excludeId: model.Id, cancellationToken);
        if (!conflictCheck.Succeeded) return conflictCheck;

        schedule.CourseId = model.CourseId;
        schedule.AcademicYearId = model.AcademicYearId;
        schedule.SemesterId = model.SemesterId;
        schedule.ExamDate = model.ExamDate;
        schedule.StartTime = model.StartTime;
        schedule.EndTime = model.EndTime;
        schedule.ExamRoomId = model.ExamRoomId;
        schedule.InvigilatorId = model.InvigilatorId;
        schedule.Status = model.Status;
        _unitOfWork.Repository<ExamSchedule>().Update(schedule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var schedule = await _unitOfWork.Repository<ExamSchedule>().GetByIdAsync(id, cancellationToken);
        if (schedule is null) return ServiceResult.Failure("Exam schedule not found.");

        _unitOfWork.Repository<ExamSchedule>().Remove(schedule);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This exam schedule has attendance records and cannot be deleted.");
        }
    }

    /// <summary>
    /// Enforces: no double-booking a room, no double-booking an invigilator, no
    /// student sitting two exams at an overlapping time, and the room must be
    /// large enough for every approved applicant. All checked before the write.
    /// </summary>
    private async Task<ServiceResult> CheckConflictsAsync(ExamScheduleFormViewModel model, int? excludeId, CancellationToken cancellationToken)
    {
        if (model.EndTime <= model.StartTime)
        {
            return ServiceResult.Failure("End time must be after the start time.");
        }

        var sameDayOverlapping = await _unitOfWork.Repository<ExamSchedule>().Query()
            .Include(s => s.Course)
            .Where(s => s.ExamDate.Date == model.ExamDate.Date)
            .Where(s => excludeId == null || s.Id != excludeId.Value)
            .Where(s => model.StartTime < s.EndTime && s.StartTime < model.EndTime)
            .ToListAsync(cancellationToken);

        var roomConflict = sameDayOverlapping.FirstOrDefault(s => s.ExamRoomId == model.ExamRoomId);
        if (roomConflict is not null)
        {
            return ServiceResult.Failure($"This room is already booked for {roomConflict.Course.Code} from {roomConflict.StartTime:hh\\:mm} to {roomConflict.EndTime:hh\\:mm} on this date.");
        }

        var invigilatorConflict = sameDayOverlapping.FirstOrDefault(s => s.InvigilatorId == model.InvigilatorId);
        if (invigilatorConflict is not null)
        {
            return ServiceResult.Failure($"This invigilator is already assigned to {invigilatorConflict.Course.Code} from {invigilatorConflict.StartTime:hh\\:mm} to {invigilatorConflict.EndTime:hh\\:mm} on this date.");
        }

        var applications = _unitOfWork.Repository<ReExamApplication>().Query();

        var ourStudentIds = await applications
            .Where(a => a.CourseId == model.CourseId && a.AcademicYearId == model.AcademicYearId && a.SemesterId == model.SemesterId && a.Status == ApplicationStatus.Approved)
            .Select(a => a.StudentId)
            .ToListAsync(cancellationToken);

        if (ourStudentIds.Count > 0 && sameDayOverlapping.Count > 0)
        {
            var overlappingCourseKeys = sameDayOverlapping
                .Select(s => new { s.CourseId, s.AcademicYearId, s.SemesterId })
                .Distinct();

            foreach (var key in overlappingCourseKeys)
            {
                var otherStudentIds = await applications
                    .Where(a => a.CourseId == key.CourseId && a.AcademicYearId == key.AcademicYearId && a.SemesterId == key.SemesterId && a.Status == ApplicationStatus.Approved)
                    .Select(a => a.StudentId)
                    .ToListAsync(cancellationToken);

                if (ourStudentIds.Intersect(otherStudentIds).Any())
                {
                    return ServiceResult.Failure("One or more students registered for this course already have another exam scheduled at an overlapping time.");
                }
            }
        }

        var room = await _unitOfWork.Repository<ExamRoom>().GetByIdAsync(model.ExamRoomId, cancellationToken);
        if (room is not null && ourStudentIds.Count > room.Capacity)
        {
            return ServiceResult.Failure($"Room capacity ({room.Capacity}) is less than the number of registered students ({ourStudentIds.Count}). Choose a larger room.");
        }

        return ServiceResult.Success();
    }

    private async Task PopulateOptionsAsync(ExamScheduleFormViewModel model, CancellationToken cancellationToken)
    {
        var courses = await _unitOfWork.Repository<Course>().GetAllAsync(cancellationToken);
        var academicYears = await _unitOfWork.Repository<AcademicYear>().GetAllAsync(cancellationToken);
        var semesters = await _unitOfWork.Repository<Semester>().GetAllAsync(cancellationToken);
        var rooms = await _unitOfWork.Repository<ExamRoom>().FindAsync(r => r.Status == RoomStatus.Available, cancellationToken);
        var invigilators = await _unitOfWork.Repository<Invigilator>().FindAsync(i => i.Status == InvigilatorStatus.Active, cancellationToken);

        model.CourseOptions = courses.OrderBy(c => c.Code).Select(c => new SelectOption { Id = c.Id, Name = $"{c.Code} - {c.Name}" }).ToList();
        model.AcademicYearOptions = academicYears.OrderByDescending(y => y.StartDate).Select(y => new SelectOption { Id = y.Id, Name = y.Name }).ToList();
        model.SemesterOptions = semesters.OrderBy(s => s.Name).Select(s => new SelectOption { Id = s.Id, Name = s.Name }).ToList();
        model.ExamRoomOptions = rooms.OrderBy(r => r.Building).ThenBy(r => r.RoomNumber).Select(r => new SelectOption { Id = r.Id, Name = $"{r.Building} {r.RoomNumber} (cap. {r.Capacity})" }).ToList();
        model.InvigilatorOptions = invigilators.OrderBy(i => i.FullName).Select(i => new SelectOption { Id = i.Id, Name = i.FullName }).ToList();
    }
}
