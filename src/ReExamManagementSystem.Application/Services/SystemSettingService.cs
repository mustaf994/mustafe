using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class SystemSettingService : ISystemSettingService
{
    private readonly IUnitOfWork _unitOfWork;

    public SystemSettingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<SystemSettingFormViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _unitOfWork.Repository<SystemSetting>().GetAllAsync(cancellationToken);
        return settings.OrderBy(s => s.Key).Select(s => new SystemSettingFormViewModel
        {
            Id = s.Id,
            Key = s.Key,
            Value = s.Value,
            Description = s.Description
        }).ToList();
    }

    public async Task<SystemSettingFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.Repository<SystemSetting>().GetByIdAsync(id, cancellationToken);
        return setting is null ? null : new SystemSettingFormViewModel { Id = setting.Id, Key = setting.Key, Value = setting.Value, Description = setting.Description };
    }

    public async Task<ServiceResult> CreateAsync(SystemSettingFormViewModel model, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Repository<SystemSetting>().AnyAsync(s => s.Key == model.Key, cancellationToken))
        {
            return ServiceResult.Failure("A setting with this key already exists.");
        }

        await _unitOfWork.Repository<SystemSetting>().AddAsync(new SystemSetting
        {
            Key = model.Key,
            Value = model.Value,
            Description = model.Description
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(SystemSettingFormViewModel model, CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.Repository<SystemSetting>().GetByIdAsync(model.Id, cancellationToken);
        if (setting is null) return ServiceResult.Failure("Setting not found.");

        if (await _unitOfWork.Repository<SystemSetting>().AnyAsync(s => s.Key == model.Key && s.Id != model.Id, cancellationToken))
        {
            return ServiceResult.Failure("A setting with this key already exists.");
        }

        setting.Key = model.Key;
        setting.Value = model.Value;
        setting.Description = model.Description;
        _unitOfWork.Repository<SystemSetting>().Update(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var setting = await _unitOfWork.Repository<SystemSetting>().GetByIdAsync(id, cancellationToken);
        if (setting is null) return ServiceResult.Failure("Setting not found.");

        _unitOfWork.Repository<SystemSetting>().Remove(setting);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
