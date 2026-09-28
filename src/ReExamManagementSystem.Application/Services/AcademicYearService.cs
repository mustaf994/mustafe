using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class AcademicYearService : IAcademicYearService
{
    private readonly IUnitOfWork _unitOfWork;

    public AcademicYearService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<AcademicYearListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<AcademicYear>().Query()
            .Include(y => y.Semesters)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(y => y.Name.Contains(search));
        }

        return await query
            .OrderByDescending(y => y.StartDate)
            .Select(y => new AcademicYearListItemViewModel
            {
                Id = y.Id,
                Name = y.Name,
                StartDate = y.StartDate,
                EndDate = y.EndDate,
                IsActive = y.IsActive,
                SemesterCount = y.Semesters.Count
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<AcademicYearFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var year = await _unitOfWork.Repository<AcademicYear>().GetByIdAsync(id, cancellationToken);
        if (year is null) return null;

        return new AcademicYearFormViewModel
        {
            Id = year.Id,
            Name = year.Name,
            StartDate = year.StartDate,
            EndDate = year.EndDate,
            IsActive = year.IsActive
        };
    }

    public async Task<ServiceResult> CreateAsync(AcademicYearFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.EndDate <= model.StartDate)
        {
            return ServiceResult.Failure("End date must be after the start date.");
        }

        if (await _unitOfWork.Repository<AcademicYear>().AnyAsync(y => y.Name == model.Name, cancellationToken))
        {
            return ServiceResult.Failure("An academic year with this name already exists.");
        }

        if (model.IsActive)
        {
            await DeactivateAllAsync(cancellationToken);
        }

        await _unitOfWork.Repository<AcademicYear>().AddAsync(new AcademicYear
        {
            Name = model.Name,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            IsActive = model.IsActive
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(AcademicYearFormViewModel model, CancellationToken cancellationToken = default)
    {
        var year = await _unitOfWork.Repository<AcademicYear>().GetByIdAsync(model.Id, cancellationToken);
        if (year is null) return ServiceResult.Failure("Academic year not found.");

        if (model.EndDate <= model.StartDate)
        {
            return ServiceResult.Failure("End date must be after the start date.");
        }

        if (await _unitOfWork.Repository<AcademicYear>().AnyAsync(y => y.Name == model.Name && y.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("An academic year with this name already exists.");
        }

        if (model.IsActive && !year.IsActive)
        {
            await DeactivateAllAsync(cancellationToken);
        }

        year.Name = model.Name;
        year.StartDate = model.StartDate;
        year.EndDate = model.EndDate;
        year.IsActive = model.IsActive;
        _unitOfWork.Repository<AcademicYear>().Update(year);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var year = await _unitOfWork.Repository<AcademicYear>().Query()
            .Include(y => y.Semesters)
            .SingleOrDefaultAsync(y => y.Id == id, cancellationToken);

        if (year is null) return ServiceResult.Failure("Academic year not found.");

        if (year.Semesters.Count > 0)
        {
            return ServiceResult.Failure("This academic year has semesters assigned to it and cannot be deleted.");
        }

        _unitOfWork.Repository<AcademicYear>().Remove(year);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private async Task DeactivateAllAsync(CancellationToken cancellationToken)
    {
        var activeYears = await _unitOfWork.Repository<AcademicYear>().FindAsync(y => y.IsActive, cancellationToken);
        foreach (var activeYear in activeYears)
        {
            activeYear.IsActive = false;
            _unitOfWork.Repository<AcademicYear>().Update(activeYear);
        }
    }
}
