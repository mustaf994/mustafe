using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetAsync(CancellationToken cancellationToken = default);
}
