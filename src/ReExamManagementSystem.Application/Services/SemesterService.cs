using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;

    public SemesterService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SemesterListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Semester>().Query()
            .Include(s => s.AcademicYear)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.Contains(search) || s.AcademicYear.Name.Contains(search));
        }

        return await query
            .OrderByDescending(s => s.AcademicYear.StartDate).ThenBy(s => s.Name)
            .Select(s => new SemesterListItemViewModel
            {
                Id = s.Id,
                Name = s.Name,
                AcademicYearName = s.AcademicYear.Name,
                StartDate = s.StartDate,
                EndDate = s.EndDate,
                IsActive = s.IsActive
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<SemesterFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default)
    {
        return new SemesterFormViewModel { AcademicYearOptions = await GetAcademicYearOptionsAsync(cancellationToken) };
    }

    public async Task<SemesterFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var semester = await _unitOfWork.Repository<Semester>().GetByIdAsync(id, cancellationToken);
        if (semester is null) return null;

        return new SemesterFormViewModel
        {
            Id = semester.Id,
            Name = semester.Name,
            StartDate = semester.StartDate,
            EndDate = semester.EndDate,
            IsActive = semester.IsActive,
            AcademicYearId = semester.AcademicYearId,
            AcademicYearOptions = await GetAcademicYearOptionsAsync(cancellationToken)
        };
    }

    public async Task<ServiceResult> CreateAsync(SemesterFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.EndDate <= model.StartDate)
        {
            return ServiceResult.Failure("End date must be after the start date.");
        }

        if (await _unitOfWork.Repository<Semester>().AnyAsync(s => s.AcademicYearId == model.AcademicYearId && s.Name == model.Name, cancellationToken))
        {
            return ServiceResult.Failure("This academic year already has a semester with this name.");
        }

        await _unitOfWork.Repository<Semester>().AddAsync(new Semester
        {
            Name = model.Name,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            IsActive = model.IsActive,
            AcademicYearId = model.AcademicYearId
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(SemesterFormViewModel model, CancellationToken cancellationToken = default)
    {
        var semester = await _unitOfWork.Repository<Semester>().GetByIdAsync(model.Id, cancellationToken);
        if (semester is null) return ServiceResult.Failure("Semester not found.");

        if (model.EndDate <= model.StartDate)
        {
            return ServiceResult.Failure("End date must be after the start date.");
        }

        if (await _unitOfWork.Repository<Semester>().AnyAsync(s => s.AcademicYearId == model.AcademicYearId && s.Name == model.Name && s.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("This academic year already has a semester with this name.");
        }

        semester.Name = model.Name;
        semester.StartDate = model.StartDate;
        semester.EndDate = model.EndDate;
        semester.IsActive = model.IsActive;
        semester.AcademicYearId = model.AcademicYearId;
        _unitOfWork.Repository<Semester>().Update(semester);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var semester = await _unitOfWork.Repository<Semester>().GetByIdAsync(id, cancellationToken);
        if (semester is null) return ServiceResult.Failure("Semester not found.");

        _unitOfWork.Repository<Semester>().Remove(semester);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This semester is referenced by other records (courses, students, results, etc.) and cannot be deleted.");
        }
    }

    private async Task<List<SelectOption>> GetAcademicYearOptionsAsync(CancellationToken cancellationToken)
    {
        var years = await _unitOfWork.Repository<AcademicYear>().GetAllAsync(cancellationToken);
        return years.OrderByDescending(y => y.StartDate).Select(y => new SelectOption { Id = y.Id, Name = y.Name }).ToList();
    }
}
