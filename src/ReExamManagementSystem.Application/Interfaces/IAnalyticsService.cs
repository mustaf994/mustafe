using ReExamManagementSystem.Application.ViewModels.Shared;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsViewModel> GetAsync(CancellationToken cancellationToken = default);
}
