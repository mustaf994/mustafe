using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class ProgramService : IProgramService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProgramService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProgramListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<AcademicProgram>().Query()
            .Include(p => p.Department)
            .Include(p => p.Students)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.Code.Contains(search) || p.Department.Name.Contains(search));
        }

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new ProgramListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                DurationYears = p.DurationYears,
                DepartmentName = p.Department.Name,
                Status = p.Status,
                StudentCount = p.Students.Count
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<ProgramFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default)
    {
        return new ProgramFormViewModel { DepartmentOptions = await GetDepartmentOptionsAsync(cancellationToken) };
    }

    public async Task<ProgramFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var program = await _unitOfWork.Repository<AcademicProgram>().GetByIdAsync(id, cancellationToken);
        if (program is null) return null;

        return new ProgramFormViewModel
        {
            Id = program.Id,
            Name = program.Name,
            Code = program.Code,
            DurationYears = program.DurationYears,
            Status = program.Status,
            DepartmentId = program.DepartmentId,
            DepartmentOptions = await GetDepartmentOptionsAsync(cancellationToken)
        };
    }

    public async Task<ServiceResult> CreateAsync(ProgramFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<AcademicProgram>().AnyAsync(p => p.Code == model.Code, cancellationToken))
        {
            return ServiceResult.Failure("A program with this code already exists.");
        }

        await _unitOfWork.Repository<AcademicProgram>().AddAsync(new AcademicProgram
        {
            Name = model.Name,
            Code = model.Code,
            DurationYears = model.DurationYears,
            Status = model.Status,
            DepartmentId = model.DepartmentId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(ProgramFormViewModel model, CancellationToken cancellationToken = default)
    {
        var program = await _unitOfWork.Repository<AcademicProgram>().GetByIdAsync(model.Id, cancellationToken);
        if (program is null) return ServiceResult.Failure("Program not found.");

        if (await _unitOfWork.Repository<AcademicProgram>().AnyAsync(p => p.Code == model.Code && p.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("A program with this code already exists.");
        }

        program.Name = model.Name;
        program.Code = model.Code;
        program.DurationYears = model.DurationYears;
        program.Status = model.Status;
        program.DepartmentId = model.DepartmentId;
        _unitOfWork.Repository<AcademicProgram>().Update(program);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var program = await _unitOfWork.Repository<AcademicProgram>().GetByIdAsync(id, cancellationToken);
        if (program is null) return ServiceResult.Failure("Program not found.");

        _unitOfWork.Repository<AcademicProgram>().Remove(program);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This program has students enrolled and cannot be deleted.");
        }
    }

    private async Task<List<SelectOption>> GetDepartmentOptionsAsync(CancellationToken cancellationToken)
    {
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync(cancellationToken);
        return departments.OrderBy(d => d.Name).Select(d => new SelectOption { Id = d.Id, Name = d.Name }).ToList();
    }
}
