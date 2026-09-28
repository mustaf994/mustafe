using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class GradeCalculationService : IGradeCalculationService
{
    private readonly IUnitOfWork _unitOfWork;

    public GradeCalculationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(string Grade, decimal GradePoint, ResultStatus Status)> CalculateAsync(decimal mark, CancellationToken cancellationToken = default)
    {
        var rules = await _unitOfWork.Repository<GradingRule>().GetAllAsync(cancellationToken);
        var rule = rules
            .Where(r => mark >= r.MinMark && mark <= r.MaxMark)
            .OrderBy(r => r.MinMark)
            .FirstOrDefault();

        if (rule is null)
        {
            throw new InvalidOperationException(
                $"No grading rule covers a mark of {mark}. An administrator must configure grading rules covering the full 0-100 range.");
        }

        return (rule.Grade, rule.GradePoint, rule.IsPassing ? ResultStatus.Pass : ResultStatus.Fail);
    }
}
