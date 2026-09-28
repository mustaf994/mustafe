using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Student.Controllers;

public class ApplicationController : StudentAreaControllerBase
{
    private readonly IReExamApplicationService _applicationService;
    private readonly IStudentService _studentService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationController(
        IReExamApplicationService applicationService,
        IStudentService studentService,
        IAuditLogService auditLog,
        UserManager<ApplicationUser> userManager)
    {
        _applicationService = applicationService;
        _studentService = studentService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Applications";
        var studentId = await CurrentStudentIdAsync();
        if (studentId is null) return Forbid();

        return View(await _applicationService.GetMyApplicationsAsync(studentId.Value));
    }

    public async Task<IActionResult> Eligible()
    {
        ViewData["Title"] = "Eligible Courses";
        var studentId = await CurrentStudentIdAsync();
        if (studentId is null) return Forbid();

        return View(await _applicationService.GetEligibleCoursesForStudentAsync(studentId.Value));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int eligibilityId)
    {
        var studentId = await CurrentStudentIdAsync();
        if (studentId is null) return Forbid();

        var result = await _applicationService.SubmitAsync(studentId.Value, eligibilityId);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Your re-exam application was submitted.";
            var userId = _userManager.GetUserId(User);
            await _auditLog.LogAsync(userId, User.Identity?.Name, "Submit", "ReExamApplication", null, HttpContext.Connection.RemoteIpAddress?.ToString(), $"EligibilityId={eligibilityId}");
        }

        return RedirectToAction(nameof(Eligible));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var studentId = await CurrentStudentIdAsync();
        if (studentId is null) return Forbid();

        var result = await _applicationService.CancelAsync(studentId.Value, id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Your application was cancelled.";
            var userId = _userManager.GetUserId(User);
            await _auditLog.LogAsync(userId, User.Identity?.Name, "Cancel", "ReExamApplication", id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), null);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<int?> CurrentStudentIdAsync()
    {
        var userId = _userManager.GetUserId(User);
        return userId is null ? null : await _studentService.GetStudentIdByUserIdAsync(userId);
    }
}
