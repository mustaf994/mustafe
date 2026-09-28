using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;

    public DepartmentService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<DepartmentListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Department>().Query()
            .Include(d => d.Faculty)
            .Include(d => d.Programs)
            .Include(d => d.Courses)
            .Include(d => d.Students)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.Name.Contains(search) || d.Code.Contains(search) || d.Faculty.Name.Contains(search));
        }

        return await query
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentListItemViewModel
            {
                Id = d.Id,
                Name = d.Name,
                Code = d.Code,
                FacultyName = d.Faculty.Name,
                ProgramCount = d.Programs.Count,
                CourseCount = d.Courses.Count,
                StudentCount = d.Students.Count
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<DepartmentFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default)
    {
        return new DepartmentFormViewModel { FacultyOptions = await GetFacultyOptionsAsync(cancellationToken) };
    }

    public async Task<DepartmentFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(id, cancellationToken);
        if (department is null) return null;

        return new DepartmentFormViewModel
        {
            Id = department.Id,
            Name = department.Name,
            Code = department.Code,
            FacultyId = department.FacultyId,
            FacultyOptions = await GetFacultyOptionsAsync(cancellationToken)
        };
    }

    public async Task<ServiceResult> CreateAsync(DepartmentFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<Department>().AnyAsync(d => d.Code == model.Code, cancellationToken))
        {
            return ServiceResult.Failure("A department with this code already exists.");
        }

        await _unitOfWork.Repository<Department>().AddAsync(new Department
        {
            Name = model.Name,
            Code = model.Code,
            FacultyId = model.FacultyId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(DepartmentFormViewModel model, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Repository<Department>().GetByIdAsync(model.Id, cancellationToken);
        if (department is null) return ServiceResult.Failure("Department not found.");

        if (await _unitOfWork.Repository<Department>().AnyAsync(d => d.Code == model.Code && d.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("A department with this code already exists.");
        }

        department.Name = model.Name;
        department.Code = model.Code;
        department.FacultyId = model.FacultyId;
        _unitOfWork.Repository<Department>().Update(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var department = await _unitOfWork.Repository<Department>().Query()
            .Include(d => d.Programs)
            .Include(d => d.Courses)
            .Include(d => d.Students)
            .SingleOrDefaultAsync(d => d.Id == id, cancellationToken);

        if (department is null) return ServiceResult.Failure("Department not found.");

        if (department.Programs.Count > 0 || department.Courses.Count > 0 || department.Students.Count > 0)
        {
            return ServiceResult.Failure("This department has programs, courses or students assigned to it and cannot be deleted.");
        }

        _unitOfWork.Repository<Department>().Remove(department);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private async Task<List<SelectOption>> GetFacultyOptionsAsync(CancellationToken cancellationToken)
    {
        var faculties = await _unitOfWork.Repository<Faculty>().GetAllAsync(cancellationToken);
        return faculties.OrderBy(f => f.Name).Select(f => new SelectOption { Id = f.Id, Name = f.Name }).ToList();
    }
}
