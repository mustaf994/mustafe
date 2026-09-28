using ReExamManagementSystem.Application.ViewModels.Student;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IStudentDashboardService
{
    Task<StudentDashboardViewModel> GetAsync(int studentId, string userId, CancellationToken cancellationToken = default);
}
