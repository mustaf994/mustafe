using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Student;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class StudentResultService : IStudentResultService
{
    private readonly IUnitOfWork _unitOfWork;

    public StudentResultService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PublishedResultViewModel>> GetPublishedResultsAsync(int studentId, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Repository<ReExamResult>().Query()
            .Include(r => r.Course)
            .Where(r => r.StudentId == studentId && r.IsPublished)
            .OrderByDescending(r => r.PublishedAt)
            .Select(r => new PublishedResultViewModel
            {
                CourseCode = r.Course.Code,
                CourseName = r.Course.Name,
                OriginalMark = r.OriginalMark,
                ReExamMark = r.ReExamMark,
                FinalMark = r.FinalMark,
                Grade = r.Grade,
                GradePoint = r.GradePoint,
                Status = r.Status,
                PublishedAt = r.PublishedAt
            })
            .ToListAsync(cancellationToken);
    }
}
