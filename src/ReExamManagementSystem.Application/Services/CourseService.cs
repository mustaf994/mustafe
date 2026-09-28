using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;

    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<CourseListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Course>().Query()
            .Include(c => c.Department)
            .Include(c => c.AcademicYear)
            .Include(c => c.Semester)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.Code.Contains(search) || c.Name.Contains(search) || c.Department.Name.Contains(search));
        }

        return await query
            .OrderBy(c => c.Code)
            .Select(c => new CourseListItemViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                CreditHours = c.CreditHours,
                Level = c.Level,
                DepartmentName = c.Department.Name,
                AcademicYearName = c.AcademicYear.Name,
                SemesterName = c.Semester.Name,
                Status = c.Status
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<CourseFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default)
    {
        var model = new CourseFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<CourseFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id, cancellationToken);
        if (course is null) return null;

        var model = new CourseFormViewModel
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            CreditHours = course.CreditHours,
            Level = course.Level,
            Status = course.Status,
            DepartmentId = course.DepartmentId,
            AcademicYearId = course.AcademicYearId,
            SemesterId = course.SemesterId
        };
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<ServiceResult> CreateAsync(CourseFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<Course>().AnyAsync(
                c => c.Code == model.Code && c.AcademicYearId == model.AcademicYearId && c.SemesterId == model.SemesterId,
                cancellationToken))
        {
            return ServiceResult.Failure("This course code already exists for the selected academic year and semester.");
        }

        await _unitOfWork.Repository<Course>().AddAsync(new Course
        {
            Code = model.Code,
            Name = model.Name,
            CreditHours = model.CreditHours,
            Level = model.Level,
            Status = model.Status,
            DepartmentId = model.DepartmentId,
            AcademicYearId = model.AcademicYearId,
            SemesterId = model.SemesterId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(CourseFormViewModel model, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(model.Id, cancellationToken);
        if (course is null) return ServiceResult.Failure("Course not found.");

        if (await _unitOfWork.Repository<Course>().AnyAsync(
                c => c.Code == model.Code && c.AcademicYearId == model.AcademicYearId && c.SemesterId == model.SemesterId && c.Id != model.Id,
                cancellationToken))
        {
            return ServiceResult.Failure("This course code already exists for the selected academic year and semester.");
        }

        course.Code = model.Code;
        course.Name = model.Name;
        course.CreditHours = model.CreditHours;
        course.Level = model.Level;
        course.Status = model.Status;
        course.DepartmentId = model.DepartmentId;
        course.AcademicYearId = model.AcademicYearId;
        course.SemesterId = model.SemesterId;
        _unitOfWork.Repository<Course>().Update(course);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await _unitOfWork.Repository<Course>().GetByIdAsync(id, cancellationToken);
        if (course is null) return ServiceResult.Failure("Course not found.");

        _unitOfWork.Repository<Course>().Remove(course);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This course is referenced by other records (results, applications, schedules, etc.) and cannot be deleted.");
        }
    }

    private async Task PopulateOptionsAsync(CourseFormViewModel model, CancellationToken cancellationToken)
    {
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync(cancellationToken);
        var academicYears = await _unitOfWork.Repository<AcademicYear>().GetAllAsync(cancellationToken);
        var semesters = await _unitOfWork.Repository<Semester>().Query().Include(s => s.AcademicYear).ToListAsync(cancellationToken);

        model.DepartmentOptions = departments.OrderBy(d => d.Name).Select(d => new SelectOption { Id = d.Id, Name = d.Name }).ToList();
        model.AcademicYearOptions = academicYears.OrderByDescending(y => y.StartDate).Select(y => new SelectOption { Id = y.Id, Name = y.Name }).ToList();
        model.SemesterOptions = semesters.OrderByDescending(s => s.AcademicYear.StartDate).ThenBy(s => s.Name)
            .Select(s => new SelectOption { Id = s.Id, Name = $"{s.AcademicYear.Name} - {s.Name}" }).ToList();
    }
}
