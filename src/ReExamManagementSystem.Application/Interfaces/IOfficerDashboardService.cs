using ReExamManagementSystem.Application.ViewModels.Officer;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IOfficerDashboardService
{
    Task<OfficerDashboardViewModel> GetAsync(CancellationToken cancellationToken = default);
}
