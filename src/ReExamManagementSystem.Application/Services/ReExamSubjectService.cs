using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class ReExamSubjectService : IReExamSubjectService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReExamSubjectService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ReExamSubjectListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<ReExamSubject>().Query()
            .Include(s => s.Course).ThenInclude(c => c.Department)
            .Include(s => s.AcademicYear)
            .Include(s => s.Semester)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Course.Code.Contains(search) || s.Course.Name.Contains(search));
        }

        var applications = _unitOfWork.Repository<ReExamApplication>().Query();

        return await query
            .OrderByDescending(s => s.AcademicYear.StartDate).ThenBy(s => s.Course.Code)
            .Select(s => new ReExamSubjectListItemViewModel
            {
                Id = s.Id,
                CourseCode = s.Course.Code,
                CourseName = s.Course.Name,
                DepartmentName = s.Course.Department.Name,
                CreditHours = s.Course.CreditHours,
                AcademicYearName = s.AcademicYear.Name,
                SemesterName = s.Semester.Name,
                IsActive = s.IsActive,
                RegisteredStudentCount = applications.Count(a =>
                    a.CourseId == s.CourseId &&
                    a.AcademicYearId == s.AcademicYearId &&
                    a.SemesterId == s.SemesterId &&
                    a.Status != ApplicationStatus.Rejected &&
                    a.Status != ApplicationStatus.Cancelled)
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<ReExamSubjectFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default)
    {
        var model = new ReExamSubjectFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<ReExamSubjectFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var subject = await _unitOfWork.Repository<ReExamSubject>().GetByIdAsync(id, cancellationToken);
        if (subject is null) return null;

        var model = new ReExamSubjectFormViewModel
        {
            Id = subject.Id,
            CourseId = subject.CourseId,
            AcademicYearId = subject.AcademicYearId,
            SemesterId = subject.SemesterId,
            IsActive = subject.IsActive
        };
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<ServiceResult> CreateAsync(ReExamSubjectFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<ReExamSubject>().AnyAsync(
                s => s.CourseId == model.CourseId && s.AcademicYearId == model.AcademicYearId && s.SemesterId == model.SemesterId,
                cancellationToken))
        {
            return ServiceResult.Failure("This course is already open for re-exam in the selected term.");
        }

        await _unitOfWork.Repository<ReExamSubject>().AddAsync(new ReExamSubject
        {
            CourseId = model.CourseId,
            AcademicYearId = model.AcademicYearId,
            SemesterId = model.SemesterId,
            IsActive = model.IsActive
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(ReExamSubjectFormViewModel model, CancellationToken cancellationToken = default)
    {
        var subject = await _unitOfWork.Repository<ReExamSubject>().GetByIdAsync(model.Id, cancellationToken);
        if (subject is null) return ServiceResult.Failure("Re-exam subject not found.");

        if (await _unitOfWork.Repository<ReExamSubject>().AnyAsync(
                s => s.CourseId == model.CourseId && s.AcademicYearId == model.AcademicYearId && s.SemesterId == model.SemesterId && s.Id != model.Id,
                cancellationToken))
        {
            return ServiceResult.Failure("This course is already open for re-exam in the selected term.");
        }

        subject.CourseId = model.CourseId;
        subject.AcademicYearId = model.AcademicYearId;
        subject.SemesterId = model.SemesterId;
        subject.IsActive = model.IsActive;
        _unitOfWork.Repository<ReExamSubject>().Update(subject);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var subject = await _unitOfWork.Repository<ReExamSubject>().GetByIdAsync(id, cancellationToken);
        if (subject is null) return ServiceResult.Failure("Re-exam subject not found.");

        _unitOfWork.Repository<ReExamSubject>().Remove(subject);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private async Task PopulateOptionsAsync(ReExamSubjectFormViewModel model, CancellationToken cancellationToken)
    {
        var courses = await _unitOfWork.Repository<Course>().GetAllAsync(cancellationToken);
        var academicYears = await _unitOfWork.Repository<AcademicYear>().GetAllAsync(cancellationToken);
        var semesters = await _unitOfWork.Repository<Semester>().GetAllAsync(cancellationToken);

        model.CourseOptions = courses.OrderBy(c => c.Code).Select(c => new SelectOption { Id = c.Id, Name = $"{c.Code} - {c.Name}" }).ToList();
        model.AcademicYearOptions = academicYears.OrderByDescending(y => y.StartDate).Select(y => new SelectOption { Id = y.Id, Name = y.Name }).ToList();
        model.SemesterOptions = semesters.OrderBy(s => s.Name).Select(s => new SelectOption { Id = s.Id, Name = s.Name }).ToList();
    }
}
