using ReExamManagementSystem.Application.ViewModels.Admin;

namespace ReExamManagementSystem.Application.ViewModels.Shared;

public class AnalyticsViewModel
{
    public List<ChartPoint> ApplicationsByStatus { get; set; } = new();
    public List<ChartPoint> ResultsByGrade { get; set; } = new();
    public List<ChartPoint> PassVsFail { get; set; } = new();
    public List<ChartPoint> MostRepeatedCourses { get; set; } = new();
    public List<ChartPoint> StudentsByProgram { get; set; } = new();
    public List<ChartPoint> ApplicationsByAcademicYear { get; set; } = new();
}
