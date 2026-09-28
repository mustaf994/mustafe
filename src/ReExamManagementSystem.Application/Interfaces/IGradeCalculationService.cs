using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IGradeCalculationService
{
    /// <summary>
    /// Looks up the mark against the admin-configurable GradingRule bands -
    /// the single source of truth for grade/grade-point/pass-fail, so no
    /// threshold is ever hard-coded elsewhere in the app.
    /// </summary>
    Task<(string Grade, decimal GradePoint, ResultStatus Status)> CalculateAsync(decimal mark, CancellationToken cancellationToken = default);
}
