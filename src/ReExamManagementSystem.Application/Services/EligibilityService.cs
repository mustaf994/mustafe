using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class EligibilityService : IEligibilityService
{
    private readonly IUnitOfWork _unitOfWork;

    public EligibilityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<EligibleStudentViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<ReExamEligibility>().Query()
            .Include(e => e.Student)
            .Include(e => e.Course)
            .Include(e => e.StudentResult)
            .Include(e => e.AcademicYear)
            .Include(e => e.Semester)
            .Where(e => e.Status == EligibilityStatus.Eligible)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>
                e.Student.FullName.Contains(search) ||
                e.Student.StudentNumber.Contains(search) ||
                e.Course.Code.Contains(search) ||
                e.Course.Name.Contains(search));
        }

        var applicationEligibilityIds = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Where(a => a.Status == ApplicationStatus.Pending || a.Status == ApplicationStatus.Approved)
            .Select(a => a.ReExamEligibilityId)
            .ToListAsync(cancellationToken);

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new EligibleStudentViewModel
            {
                EligibilityId = e.Id,
                StudentNumber = e.Student.StudentNumber,
                StudentName = e.Student.FullName,
                CourseCode = e.Course.Code,
                CourseName = e.Course.Name,
                PreviousMark = e.StudentResult.Marks,
                PreviousGrade = e.StudentResult.Grade,
                AcademicYearName = e.AcademicYear.Name,
                SemesterName = e.Semester.Name,
                Reason = e.Reason,
                HasActiveApplication = applicationEligibilityIds.Contains(e.Id)
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<int> RecomputeAsync(CancellationToken cancellationToken = default)
    {
        var existingEligibilityResultIds = (await _unitOfWork.Repository<ReExamEligibility>().GetAllAsync(cancellationToken))
            .Select(e => e.StudentResultId)
            .ToHashSet();

        var failedResults = await _unitOfWork.Repository<StudentResult>().Query()
            .Where(r => r.Status == ResultStatus.Fail)
            .ToListAsync(cancellationToken);

        var newRecords = new List<ReExamEligibility>();
        foreach (var result in failedResults.Where(r => !existingEligibilityResultIds.Contains(r.Id)))
        {
            newRecords.Add(new ReExamEligibility
            {
                StudentId = result.StudentId,
                StudentResultId = result.Id,
                CourseId = result.CourseId,
                AcademicYearId = result.AcademicYearId,
                SemesterId = result.SemesterId,
                Status = EligibilityStatus.Eligible,
                Reason = $"Failed with a mark of {result.Marks:0.##} (grade {result.Grade}), below the minimum pass mark. Retake permitted."
            });
        }

        if (newRecords.Count == 0) return 0;

        await _unitOfWork.Repository<ReExamEligibility>().AddRangeAsync(newRecords, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return newRecords.Count;
    }
}
