using ReExamManagementSystem.Application.ViewModels.Student;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IStudentExamScheduleService
{
    Task<IReadOnlyList<ScheduledExamViewModel>> GetMyScheduledExamsAsync(int studentId, CancellationToken cancellationToken = default);
    Task<ExamSlipViewModel?> GetExamSlipAsync(int studentId, int scheduleId, CancellationToken cancellationToken = default);
}
