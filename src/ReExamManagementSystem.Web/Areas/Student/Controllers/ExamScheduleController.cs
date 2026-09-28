using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Student.Controllers;

public class ExamScheduleController : StudentAreaControllerBase
{
    private readonly IStudentExamScheduleService _scheduleService;
    private readonly IStudentService _studentService;
    private readonly IReportExportService _exportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExamScheduleController(
        IStudentExamScheduleService scheduleService,
        IStudentService studentService,
        IReportExportService exportService,
        UserManager<ApplicationUser> userManager)
    {
        _scheduleService = scheduleService;
        _studentService = studentService;
        _exportService = exportService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Exam Schedule";
        var studentId = await CurrentStudentIdAsync();
        if (studentId is null) return Forbid();

        return View(await _scheduleService.GetMyScheduledExamsAsync(studentId.Value));
    }

    public async Task<IActionResult> Slip(int scheduleId)
    {
        ViewData["Title"] = "Examination Slip";
        var studentId = await CurrentStudentIdAsync();
        if (studentId is null) return Forbid();

        var slip = await _scheduleService.GetExamSlipAsync(studentId.Value, scheduleId);
        return slip is null ? NotFound() : View(slip);
    }

    public async Task<IActionResult> SlipPdf(int scheduleId)
    {
        var studentId = await CurrentStudentIdAsync();
        if (studentId is null) return Forbid();

        var slip = await _scheduleService.GetExamSlipAsync(studentId.Value, scheduleId);
        if (slip is null) return NotFound();

        var pdf = _exportService.ExportExamSlipToPdf(slip);
        return File(pdf, "application/pdf", $"exam-slip-{slip.CourseCode}.pdf");
    }

    private async Task<int?> CurrentStudentIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        return userId is null ? null : await _studentService.GetStudentIdByUserIdAsync(userId);
    }
}
