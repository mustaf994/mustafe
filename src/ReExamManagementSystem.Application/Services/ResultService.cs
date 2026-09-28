using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class ResultService : IResultService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGradeCalculationService _gradeCalculationService;
    private readonly INotificationService _notificationService;

    public ResultService(IUnitOfWork unitOfWork, IGradeCalculationService gradeCalculationService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _gradeCalculationService = gradeCalculationService;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<PendingResultEntryViewModel>> GetPendingEntryAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var resultedApplicationIds = await _unitOfWork.Repository<ReExamResult>().Query()
            .Select(r => r.ReExamApplicationId)
            .ToListAsync(cancellationToken);

        var query = _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.Student)
            .Include(a => a.Course)
            .Include(a => a.ReExamEligibility).ThenInclude(e => e.StudentResult)
            .Where(a => a.Status == ApplicationStatus.Approved && !resultedApplicationIds.Contains(a.Id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.Student.FullName.Contains(search) || a.Student.StudentNumber.Contains(search) || a.Course.Code.Contains(search));
        }

        return await query
            .OrderBy(a => a.Student.FullName)
            .Select(a => new PendingResultEntryViewModel
            {
                ApplicationId = a.Id,
                StudentNumber = a.Student.StudentNumber,
                StudentName = a.Student.FullName,
                CourseCode = a.Course.Code,
                CourseName = a.Course.Name,
                OriginalMark = a.ReExamEligibility.StudentResult.Marks,
                OriginalGrade = a.ReExamEligibility.StudentResult.Grade
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<ResultEntryFormViewModel?> GetEntryFormAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.Student)
            .Include(a => a.Course)
            .Include(a => a.ReExamEligibility).ThenInclude(e => e.StudentResult)
            .SingleOrDefaultAsync(a => a.Id == applicationId && a.Status == ApplicationStatus.Approved, cancellationToken);

        if (application is null) return null;

        return new ResultEntryFormViewModel
        {
            ApplicationId = application.Id,
            StudentName = application.Student.FullName,
            StudentNumber = application.Student.StudentNumber,
            CourseCode = application.Course.Code,
            CourseName = application.Course.Name,
            OriginalMark = application.ReExamEligibility.StudentResult.Marks,
            OriginalGrade = application.ReExamEligibility.StudentResult.Grade
        };
    }

    public async Task<ServiceResult> EnterMarksAsync(int applicationId, decimal reExamMark, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.ReExamEligibility).ThenInclude(e => e.StudentResult)
            .SingleOrDefaultAsync(a => a.Id == applicationId && a.Status == ApplicationStatus.Approved, cancellationToken);

        if (application is null)
        {
            return ServiceResult.Failure("Application not found or not approved.");
        }

        if (await _unitOfWork.Repository<ReExamResult>().AnyAsync(r => r.ReExamApplicationId == applicationId, cancellationToken))
        {
            return ServiceResult.Failure("A result has already been recorded for this application.");
        }

        // Re-exam policy: the re-sit mark becomes the student's new official
        // mark for the course (the most common university re-exam policy).
        var finalMark = reExamMark;
        var (grade, gradePoint, status) = await _gradeCalculationService.CalculateAsync(finalMark, cancellationToken);

        await _unitOfWork.Repository<ReExamResult>().AddAsync(new ReExamResult
        {
            ReExamApplicationId = application.Id,
            StudentId = application.StudentId,
            CourseId = application.CourseId,
            OriginalMark = application.ReExamEligibility.StudentResult.Marks,
            ReExamMark = reExamMark,
            FinalMark = finalMark,
            Grade = grade,
            GradePoint = gradePoint,
            Status = status,
            IsVerified = false,
            IsPublished = false
        }, cancellationToken);

        application.Status = ApplicationStatus.Completed;
        _unitOfWork.Repository<ReExamApplication>().Update(application);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<PagedResult<ResultListItemViewModel>> GetPagedAsync(bool? verifiedFilter, bool? publishedFilter, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<ReExamResult>().Query()
            .Include(r => r.Student)
            .Include(r => r.Course)
            .AsQueryable();

        if (verifiedFilter.HasValue) query = query.Where(r => r.IsVerified == verifiedFilter.Value);
        if (publishedFilter.HasValue) query = query.Where(r => r.IsPublished == publishedFilter.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Student.FullName.Contains(search) || r.Student.StudentNumber.Contains(search) || r.Course.Code.Contains(search));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ResultListItemViewModel
            {
                Id = r.Id,
                StudentNumber = r.Student.StudentNumber,
                StudentName = r.Student.FullName,
                CourseCode = r.Course.Code,
                OriginalMark = r.OriginalMark,
                ReExamMark = r.ReExamMark,
                FinalMark = r.FinalMark,
                Grade = r.Grade,
                Status = r.Status,
                IsVerified = r.IsVerified,
                IsPublished = r.IsPublished
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<ResultDetailsViewModel?> GetDetailsAsync(int resultId, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Repository<ReExamResult>().Query()
            .Include(r => r.Student)
            .Include(r => r.Course)
            .SingleOrDefaultAsync(r => r.Id == resultId, cancellationToken);

        if (result is null) return null;

        return new ResultDetailsViewModel
        {
            Id = result.Id,
            StudentNumber = result.Student.StudentNumber,
            StudentName = result.Student.FullName,
            CourseCode = result.Course.Code,
            CourseName = result.Course.Name,
            OriginalMark = result.OriginalMark,
            ReExamMark = result.ReExamMark,
            FinalMark = result.FinalMark,
            Grade = result.Grade,
            GradePoint = result.GradePoint,
            Status = result.Status,
            IsVerified = result.IsVerified,
            VerifiedAt = result.VerifiedAt,
            IsPublished = result.IsPublished,
            PublishedAt = result.PublishedAt
        };
    }

    public async Task<ServiceResult> VerifyAsync(int resultId, string verifierUserId, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Repository<ReExamResult>().GetByIdAsync(resultId, cancellationToken);
        if (result is null) return ServiceResult.Failure("Result not found.");
        if (result.IsVerified) return ServiceResult.Failure("This result has already been verified.");

        result.IsVerified = true;
        result.VerifiedByUserId = verifierUserId;
        result.VerifiedAt = DateTime.UtcNow;
        _unitOfWork.Repository<ReExamResult>().Update(result);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> PublishAsync(int resultId, string publisherUserId, CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.Repository<ReExamResult>().Query()
            .Include(r => r.Student)
            .Include(r => r.Course)
            .SingleOrDefaultAsync(r => r.Id == resultId, cancellationToken);

        if (result is null) return ServiceResult.Failure("Result not found.");
        if (!result.IsVerified) return ServiceResult.Failure("This result must be verified before it can be published.");
        if (result.IsPublished) return ServiceResult.Failure("This result has already been published.");

        result.IsPublished = true;
        result.PublishedByUserId = publisherUserId;
        result.PublishedAt = DateTime.UtcNow;
        _unitOfWork.Repository<ReExamResult>().Update(result);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAsync(
            result.Student.UserId,
            "Re-exam result published",
            $"Your re-exam result for {result.Course.Code} - {result.Course.Name} has been published: {result.Grade} ({result.FinalMark}).",
            NotificationType.ResultPublished,
            "/Student/Result",
            cancellationToken);

        return ServiceResult.Success();
    }
}
