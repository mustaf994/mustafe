using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Application.ViewModels.Shared;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Domain.Interfaces;

namespace ReExamManagementSystem.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AnalyticsViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var applications = await _unitOfWork.Repository<ReExamApplication>().GetAllAsync(cancellationToken);
        var results = await _unitOfWork.Repository<ReExamResult>().GetAllAsync(cancellationToken);

        var model = new AnalyticsViewModel
        {
            ApplicationsByStatus = Enum.GetValues<ApplicationStatus>()
                .Select(status => new ChartPoint { Label = status.ToString(), Value = applications.Count(a => a.Status == status) })
                .Where(p => p.Value > 0)
                .ToList(),

            ResultsByGrade = results
                .GroupBy(r => r.Grade)
                .Select(g => new ChartPoint { Label = g.Key, Value = g.Count() })
                .OrderBy(p => p.Label)
                .ToList(),

            PassVsFail = Enum.GetValues<ResultStatus>()
                .Select(status => new ChartPoint { Label = status.ToString(), Value = results.Count(r => r.Status == status) })
                .Where(p => p.Value > 0)
                .ToList()
        };

        model.MostRepeatedCourses = await _unitOfWork.Repository<ReExamEligibility>().Query()
            .Include(e => e.Course)
            .GroupBy(e => e.Course.Code)
            .Select(g => new ChartPoint { Label = g.Key, Value = g.Count() })
            .OrderByDescending(p => p.Value)
            .Take(8)
            .ToListAsync(cancellationToken);

        model.StudentsByProgram = await _unitOfWork.Repository<Student>().Query()
            .Include(s => s.Program)
            .GroupBy(s => s.Program.Name)
            .Select(g => new ChartPoint { Label = g.Key, Value = g.Count() })
            .OrderByDescending(p => p.Value)
            .ToListAsync(cancellationToken);

        model.ApplicationsByAcademicYear = await _unitOfWork.Repository<ReExamApplication>().Query()
            .Include(a => a.AcademicYear)
            .GroupBy(a => a.AcademicYear.Name)
            .Select(g => new ChartPoint { Label = g.Key, Value = g.Count() })
            .OrderBy(p => p.Label)
            .ToListAsync(cancellationToken);

        return model;
    }
}
