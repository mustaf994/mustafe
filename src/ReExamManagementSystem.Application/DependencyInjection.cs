using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.Services;

namespace ReExamManagementSystem.Application;

/// <summary>
/// Composition root for the Application layer. Controllers never new-up a
/// service or validator directly; everything registered here is resolved
/// through constructor injection.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IFacultyService, FacultyService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IAcademicYearService, AcademicYearService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<IProgramService, ProgramService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IEligibilityService, EligibilityService>();
        services.AddScoped<IReExamSubjectService, ReExamSubjectService>();
        services.AddScoped<IReExamApplicationService, ReExamApplicationService>();
        services.AddScoped<IExamRoomService, ExamRoomService>();
        services.AddScoped<IInvigilatorService, InvigilatorService>();
        services.AddScoped<IExamScheduleService, ExamScheduleService>();
        services.AddScoped<IExamAttendanceService, ExamAttendanceService>();
        services.AddScoped<IGradeCalculationService, GradeCalculationService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<IStudentResultService, StudentResultService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        services.AddScoped<IStudentExamScheduleService, StudentExamScheduleService>();
        services.AddScoped<IGradingRuleService, GradingRuleService>();
        services.AddScoped<IOfficerDashboardService, OfficerDashboardService>();
        services.AddScoped<IStudentDashboardService, StudentDashboardService>();

        return services;
    }
}
