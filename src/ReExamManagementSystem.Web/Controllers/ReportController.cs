using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Shared;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Web.Controllers;

[Authorize(Policy = "RequireStaff")]
public class ReportController : Controller
{
    private readonly IReportService _reportService;
    private readonly IReportExportService _exportService;
    private readonly IAnalyticsService _analyticsService;

    public ReportController(IReportService reportService, IReportExportService exportService, IAnalyticsService analyticsService)
    {
        _reportService = reportService;
        _exportService = exportService;
        _analyticsService = analyticsService;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Reports";
        return View();
    }

    public async Task<IActionResult> Analytics()
    {
        ViewData["Title"] = "Analytics";
        return View(await _analyticsService.GetAsync());
    }

    public async Task<IActionResult> StudentList(int? departmentId, StudentStatus? status, string? format)
    {
        var data = await _reportService.GetStudentListReportAsync(departmentId, status);
        return await RenderAsync(data, format);
    }

    public async Task<IActionResult> EligibleStudents(string? format)
    {
        var data = await _reportService.GetEligibleStudentReportAsync();
        return await RenderAsync(data, format);
    }

    public async Task<IActionResult> Applications(ApplicationStatus? status, string? format)
    {
        var data = await _reportService.GetApplicationReportAsync(status);
        return await RenderAsync(data, format);
    }

    public async Task<IActionResult> Results(bool? published, string? format)
    {
        var data = await _reportService.GetResultReportAsync(published);
        return await RenderAsync(data, format);
    }

    public async Task<IActionResult> Timetable(string? format)
    {
        var data = await _reportService.GetExaminationTimetableReportAsync();
        return await RenderAsync(data, format);
    }

    private Task<IActionResult> RenderAsync(ReportData data, string? format)
    {
        IActionResult result = format switch
        {
            "pdf" => File(_exportService.ExportToPdf(data.Title, data.Headers, data.Rows), "application/pdf", $"{Slug(data.Title)}.pdf"),
            "excel" => File(_exportService.ExportToExcel(data.Title, data.Headers, data.Rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{Slug(data.Title)}.xlsx"),
            _ => ReportView(data)
        };

        return Task.FromResult(result);
    }

    private IActionResult ReportView(ReportData data)
    {
        ViewData["Title"] = data.Title;
        return View("Report", data);
    }

    private static string Slug(string title) => title.Replace(" ", "-").ToLowerInvariant();
}
