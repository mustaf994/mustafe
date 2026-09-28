using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class InvigilatorService : IInvigilatorService
{
    private readonly IUnitOfWork _unitOfWork;

    public InvigilatorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<InvigilatorListItemViewModel>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _unitOfWork.Repository<Invigilator>().Query()
            .Include(i => i.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i => i.FullName.Contains(search) || i.StaffId.Contains(search) || i.Email.Contains(search));
        }

        return await query
            .OrderBy(i => i.FullName)
            .Select(i => new InvigilatorListItemViewModel
            {
                Id = i.Id,
                StaffId = i.StaffId,
                FullName = i.FullName,
                Email = i.Email,
                PhoneNumber = i.PhoneNumber,
                DepartmentName = i.Department.Name,
                Status = i.Status
            })
            .ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<InvigilatorFormViewModel> GetForCreateAsync(CancellationToken cancellationToken = default)
    {
        var model = new InvigilatorFormViewModel();
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<InvigilatorFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var invigilator = await _unitOfWork.Repository<Invigilator>().GetByIdAsync(id, cancellationToken);
        if (invigilator is null) return null;

        var model = new InvigilatorFormViewModel
        {
            Id = invigilator.Id,
            StaffId = invigilator.StaffId,
            FullName = invigilator.FullName,
            Email = invigilator.Email,
            PhoneNumber = invigilator.PhoneNumber,
            DepartmentId = invigilator.DepartmentId,
            Status = invigilator.Status
        };
        await PopulateOptionsAsync(model, cancellationToken);
        return model;
    }

    public async Task<ServiceResult> CreateAsync(InvigilatorFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<Invigilator>().AnyAsync(i => i.StaffId == model.StaffId || i.Email == model.Email, cancellationToken))
        {
            return ServiceResult.Failure("An invigilator with this staff ID or email already exists.");
        }

        await _unitOfWork.Repository<Invigilator>().AddAsync(new Invigilator
        {
            StaffId = model.StaffId,
            FullName = model.FullName,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber ?? string.Empty,
            DepartmentId = model.DepartmentId,
            Status = model.Status
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(InvigilatorFormViewModel model, CancellationToken cancellationToken = default)
    {
        var invigilator = await _unitOfWork.Repository<Invigilator>().GetByIdAsync(model.Id, cancellationToken);
        if (invigilator is null) return ServiceResult.Failure("Invigilator not found.");

        if (await _unitOfWork.Repository<Invigilator>().AnyAsync(i => (i.StaffId == model.StaffId || i.Email == model.Email) && i.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("An invigilator with this staff ID or email already exists.");
        }

        invigilator.StaffId = model.StaffId;
        invigilator.FullName = model.FullName;
        invigilator.Email = model.Email;
        invigilator.PhoneNumber = model.PhoneNumber ?? string.Empty;
        invigilator.DepartmentId = model.DepartmentId;
        invigilator.Status = model.Status;
        _unitOfWork.Repository<Invigilator>().Update(invigilator);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var invigilator = await _unitOfWork.Repository<Invigilator>().GetByIdAsync(id, cancellationToken);
        if (invigilator is null) return ServiceResult.Failure("Invigilator not found.");

        _unitOfWork.Repository<Invigilator>().Remove(invigilator);
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Failure("This invigilator has exam schedules assigned and cannot be deleted.");
        }
    }

    private async Task PopulateOptionsAsync(InvigilatorFormViewModel model, CancellationToken cancellationToken)
    {
        var departments = await _unitOfWork.Repository<Department>().GetAllAsync(cancellationToken);
        model.DepartmentOptions = departments.OrderBy(d => d.Name).Select(d => new SelectOption { Id = d.Id, Name = d.Name }).ToList();
    }
}
