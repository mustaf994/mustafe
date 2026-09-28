using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Application.ViewModels.Student;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class ReExamApplicationService : IReExamApplicationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IUserAccountService _userAccountService;

    public ReExamApplicationService(IUnitOfWork unitOfWork, INotificationService notificationService, IUserAccountService userAccountService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _userAccountService = userAccountService;
    }

    public async Task<IReadOnlyList<EligibleCourseViewModel>> GetEligibleCoursesForStudentAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var activeApplicationEligibilityIds = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Where(a => a.StudentId == studentId && (a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Approved))
            .Select(a => a.ReExamEligibilityId)
            .ToListAsync(cancellationToken);

        var openSubjects = await _unitOfWork.Repository<ReExamSubject>().Query()
            .Where(s => s.IsActive)
            .Select(s => new { s.CourseId, s.AcademicYearId, s.SemesterId })
            .ToListAsync(cancellationToken);

        var eligibilities = await _unitOfWork.Repository<ReExamEligibility>().Query()
            .Include(e => e.Course)
            .Include(e => e.StudentResult)
            .Include(e => e.AcademicYear)
            .Include(e => e.Semester)
            .Where(e => e.StudentId == studentId && e.Status == EligibilityStatus.Eligible)
            .Where(e => !activeApplicationEligibilityIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        return eligibilities.Select(e => new EligibleCourseViewModel
        {
            EligibilityId = e.Id,
            CourseCode = e.Course.Code,
            CourseName = e.Course.Name,
            PreviousMark = e.StudentResult.Marks,
            PreviousGrade = e.StudentResult.Grade,
            AcademicYearName = e.AcademicYear.Name,
            SemesterName = e.Semester.Name,
            Reason = e.Reason,
            IsOpenForReExam = openSubjects.Any(s => s.CourseId == e.CourseId)
        }).ToList();
    }

    public async Task<ServiceResult> SubmitAsync(int studentId, int eligibilityId, CancellationToken cancellationToken = default)
    {
        var eligibility = await _unitOfWork.Repository<ReExamEligibility>().Query()
            .Include(e => e.Course)
            .SingleOrDefaultAsync(e => e.Id == eligibilityId && e.StudentId == studentId, cancellationToken);

        if (eligibility is null || eligibility.Status != EligibilityStatus.Eligible)
        {
            return ServiceResult.Failure("This eligibility record was not found.");
        }

        var openSubject = await _unitOfWork.Repository<ReExamSubject>()
            .SingleOrDefaultAsync(s => s.CourseId == eligibility.CourseId && s.IsActive, cancellationToken);

        if (openSubject is null)
        {
            return ServiceResult.Failure("This course is not currently open for re-exam applications.");
        }

        var duplicateExists = await _unitOfWork.Repository<ReExamApplication>().AnyAsync(a =>
            a.StudentId == studentId &&
            a.CourseId == eligibility.CourseId &&
            a.AcademicYearId == openSubject.AcademicYearId &&
            a.SemesterId == openSubject.SemesterId &&
            a.Status != ApplicationStatus.Cancelled &&
            a.Status != ApplicationStatus.Rejected, cancellationToken);

        if (duplicateExists)
        {
            return ServiceResult.Failure("You have already applied for a re-exam in this course for this academic year and semester.");
        }

        var application = new ReExamApplication
        {
            StudentId = studentId,
            CourseId = eligibility.CourseId,
            ReExamEligibilityId = eligibility.Id,
            AcademicYearId = openSubject.AcademicYearId,
            SemesterId = openSubject.SemesterId,
            ApplicationDate = DateTime.UtcNow,
            Status = ApplicationStatus.Pending
        };

        await _unitOfWork.Repository<ReExamApplication>().AddAsync(application, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var officerIds = await _userAccountService.GetUserIdsInRoleAsync(Roles.ExaminationOfficer, cancellationToken);
        foreach (var officerId in officerIds)
        {
            await _notificationService.CreateAsync(
                officerId,
                "New re-exam application",
                $"A new re-exam application was submitted for {eligibility.Course.Code}.",
                NotificationType.ApplicationSubmitted,
                "/Officer/Application/Review/" + application.Id,
                cancellationToken);
        }

        return ServiceResult.Success();
    }

    public async Task<IReadOnlyList<MyApplicationViewModel>> GetMyApplicationsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.Course)
            .Include(a => a.AcademicYear)
            .Include(a => a.Semester)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.ApplicationDate)
            .Select(a => new MyApplicationViewModel
            {
                Id = a.Id,
                CourseCode = a.Course.Code,
                CourseName = a.Course.Name,
                ApplicationDate = a.ApplicationDate,
                AcademicYearName = a.AcademicYear.Name,
                SemesterName = a.Semester.Name,
                Status = a.Status,
                RejectionReason = a.RejectionReason
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult> CancelAsync(int studentId, int applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.Repository<ReExamApplication>()
            .SingleOrDefaultAsync(a => a.Id == applicationId && a.StudentId == studentId, cancellationToken);

        if (application is null) return ServiceResult.Failure("Application not found.");
        if (application.Status != ApplicationStatus.Pending)
        {
            return ServiceResult.Failure("Only pending applications can be cancelled.");
        }

        application.Status = ApplicationStatus.Cancelled;
        _unitOfWork.Repository<ReExamApplication>().Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<PagedResult<ApplicationListItemViewModel>> GetPagedForReviewAsync(ApplicationStatus? status, string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.Student)
            .Include(a => a.Course)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a =>
                a.Student.FullName.Contains(search) ||
                a.Student.StudentNumber.Contains(search) ||
                a.Course.Code.Contains(search));
        }

        return await query
            .OrderByDescending(a => a.ApplicationDate)
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
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<ApplicationDetailsViewModel?> GetReviewDetailsAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.Student).ThenInclude(s => s.Department)
            .Include(a => a.Student).ThenInclude(s => s.Program)
            .Include(a => a.Course)
            .Include(a => a.AcademicYear)
            .Include(a => a.Semester)
            .Include(a => a.ReExamEligibility).ThenInclude(e => e.StudentResult)
            .SingleOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application is null) return null;

        return new ApplicationDetailsViewModel
        {
            Id = application.Id,
            StudentNumber = application.Student.StudentNumber,
            StudentName = application.Student.FullName,
            StudentEmail = application.Student.Email,
            DepartmentName = application.Student.Department.Name,
            ProgramName = application.Student.Program.Name,
            CourseCode = application.Course.Code,
            CourseName = application.Course.Name,
            PreviousMark = application.ReExamEligibility.StudentResult.Marks,
            PreviousGrade = application.ReExamEligibility.StudentResult.Grade,
            EligibilityReason = application.ReExamEligibility.Reason,
            ApplicationDate = application.ApplicationDate,
            AcademicYearName = application.AcademicYear.Name,
            SemesterName = application.Semester.Name,
            Status = application.Status,
            RejectionReason = application.RejectionReason,
            ReviewedAt = application.ReviewedAt
        };
    }

    public async Task<ServiceResult> ApproveAsync(int applicationId, string officerUserId, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.Student)
            .Include(a => a.Course)
            .SingleOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application is null) return ServiceResult.Failure("Application not found.");
        if (application.Status != ApplicationStatus.Pending)
        {
            return ServiceResult.Failure("Only pending applications can be approved.");
        }

        application.Status = ApplicationStatus.Approved;
        application.ReviewedByUserId = officerUserId;
        application.ReviewedAt = DateTime.UtcNow;
        _unitOfWork.Repository<ReExamApplication>().Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAsync(
            application.Student.UserId,
            "Re-exam application approved",
            $"Your re-exam application for {application.Course.Code} - {application.Course.Name} has been approved.",
            NotificationType.ApplicationApproved,
            "/Student/Application",
            cancellationToken);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RejectAsync(int applicationId, string officerUserId, string reason, CancellationToken cancellationToken = default)
    {
        var application = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.Student)
            .Include(a => a.Course)
            .SingleOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (application is null) return ServiceResult.Failure("Application not found.");
        if (application.Status != ApplicationStatus.Pending)
        {
            return ServiceResult.Failure("Only pending applications can be rejected.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return ServiceResult.Failure("A rejection reason is required.");
        }

        application.Status = ApplicationStatus.Rejected;
        application.RejectionReason = reason;
        application.ReviewedByUserId = officerUserId;
        application.ReviewedAt = DateTime.UtcNow;
        _unitOfWork.Repository<ReExamApplication>().Update(application);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notificationService.CreateAsync(
            application.Student.UserId,
            "Re-exam application rejected",
            $"Your re-exam application for {application.Course.Code} - {application.Course.Name} was rejected: {reason}",
            NotificationType.ApplicationRejected,
            "/Student/Application",
            cancellationToken);

        return ServiceResult.Success();
    }
}
