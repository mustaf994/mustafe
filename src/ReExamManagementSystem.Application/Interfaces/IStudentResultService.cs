using ReExamManagementSystem.Application.ViewModels.Student;

namespace ReExamManagementSystem.Application.Interfaces;

public interface IStudentResultService
{
    /// <summary>Only published results are ever returned - unpublished results must never be visible to students.</summary>
    Task<IReadOnlyList<PublishedResultViewModel>> GetPublishedResultsAsync(int studentId, CancellationToken cancellationToken = default);
}
