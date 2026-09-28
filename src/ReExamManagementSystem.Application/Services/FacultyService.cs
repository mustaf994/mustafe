using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class FacultyService : IFacultyService
{
    private readonly IUnitOfWork _unitOfWork;

    public FacultyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<FacultyListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Faculty>().Query()
            .Include(f => f.Departments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(f => f.Name.Contains(search) || f.Code.Contains(search));
        }

        return await query
            .OrderBy(f => f.Name)
            .Select(f => new FacultyListItemViewModel
            {
                Id = f.Id,
                Name = f.Name,
                Code = f.Code,
                DepartmentCount = f.Departments.Count
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<FacultyFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var faculty = await _unitOfWork.Repository<Faculty>().GetByIdAsync(id, cancellationToken);
        return faculty is null
            ? null
            : new FacultyFormViewModel { Id = faculty.Id, Name = faculty.Name, Code = faculty.Code };
    }

    public async Task<ServiceResult> CreateAsync(FacultyFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<Faculty>().AnyAsync(f => f.Code == model.Code, cancellationToken))
        {
            return ServiceResult.Failure("A faculty with this code already exists.");
        }

        await _unitOfWork.Repository<Faculty>().AddAsync(new Faculty { Name = model.Name, Code = model.Code }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(FacultyFormViewModel model, CancellationToken cancellationToken = default)
    {
        var faculty = await _unitOfWork.Repository<Faculty>().GetByIdAsync(model.Id, cancellationToken);
        if (faculty is null)
        {
            return ServiceResult.Failure("Faculty not found.");
        }

        if (await _unitOfWork.Repository<Faculty>().AnyAsync(f => f.Code == model.Code && f.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("A faculty with this code already exists.");
        }

        faculty.Name = model.Name;
        faculty.Code = model.Code;
        _unitOfWork.Repository<Faculty>().Update(faculty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var faculty = await _unitOfWork.Repository<Faculty>().Query()
            .Include(f => f.Departments)
            .SingleOrDefaultAsync(f => f.Id == id, cancellationToken);

        if (faculty is null)
        {
            return ServiceResult.Failure("Faculty not found.");
        }

        if (faculty.Departments.Count > 0)
        {
            return ServiceResult.Failure("This faculty has departments assigned to it and cannot be deleted.");
        }

        _unitOfWork.Repository<Faculty>().Remove(faculty);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
