using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class GradingRuleService : IGradingRuleService
{
    private readonly IUnitOfWork _unitOfWork;

    public GradingRuleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<GradingRuleFormViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rules = await _unitOfWork.Repository<GradingRule>().GetAllAsync(cancellationToken);
        return rules.OrderByDescending(r => r.MinMark).Select(ToViewModel).ToList();
    }

    public async Task<GradingRuleFormViewModel?> GetForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        var rule = await _unitOfWork.Repository<GradingRule>().GetByIdAsync(id, cancellationToken);
        return rule is null ? null : ToViewModel(rule);
    }

    public async Task<ServiceResult> CreateAsync(GradingRuleFormViewModel model, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateAsync(model, excludeId: null, cancellationToken);
        if (!validation.Succeeded) return validation;

        await _unitOfWork.Repository<GradingRule>().AddAsync(new GradingRule
        {
            MinMark = model.MinMark,
            MaxMark = model.MaxMark,
            Grade = model.Grade,
            GradePoint = model.GradePoint,
            IsPassing = model.IsPassing,
            Description = model.Description
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateAsync(GradingRuleFormViewModel model, CancellationToken cancellationToken = default)
    {
        var rule = await _unitOfWork.Repository<GradingRule>().GetByIdAsync(model.Id, cancellationToken);
        if (rule is null) return ServiceResult.Failure("Grading rule not found.");

        var validation = await ValidateAsync(model, excludeId: model.Id, cancellationToken);
        if (!validation.Succeeded) return validation;

        rule.MinMark = model.MinMark;
        rule.MaxMark = model.MaxMark;
        rule.Grade = model.Grade;
        rule.GradePoint = model.GradePoint;
        rule.IsPassing = model.IsPassing;
        rule.Description = model.Description;
        _unitOfWork.Repository<GradingRule>().Update(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var rule = await _unitOfWork.Repository<GradingRule>().GetByIdAsync(id, cancellationToken);
        if (rule is null) return ServiceResult.Failure("Grading rule not found.");

        _unitOfWork.Repository<GradingRule>().Remove(rule);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    private async Task<ServiceResult> ValidateAsync(GradingRuleFormViewModel model, int? excludeId, CancellationToken cancellationToken)
    {
        if (model.MaxMark <= model.MinMark)
        {
            return ServiceResult.Failure("Maximum mark must be greater than the minimum mark.");
        }

        var others = await _unitOfWork.Repository<GradingRule>().GetAllAsync(cancellationToken);
        var overlaps = others.Any(r =>
            r.Id != (excludeId ?? 0) &&
            model.MinMark <= r.MaxMark &&
            r.MinMark <= model.MaxMark);

        if (overlaps)
        {
            return ServiceResult.Failure("This mark range overlaps with an existing grading rule.");
        }

        return ServiceResult.Success();
    }

    private static GradingRuleFormViewModel ToViewModel(GradingRule rule) => new()
    {
        Id = rule.Id,
        MinMark = rule.MinMark,
        MaxMark = rule.MaxMark,
        Grade = rule.Grade,
        GradePoint = rule.GradePoint,
        IsPassing = rule.IsPassing,
        Description = rule.Description
    };
}
