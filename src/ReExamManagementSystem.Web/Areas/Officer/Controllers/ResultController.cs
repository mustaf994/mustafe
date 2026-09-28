using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Officer.Controllers;

public class ResultController : OfficerControllerBase
{
    private readonly IResultService _resultService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResultController(IResultService resultService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _resultService = resultService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Enter Re-Exam Marks";
        ViewData["Search"] = search;
        return View(await _resultService.GetPendingEntryAsync(search, page, 10));
    }

    [HttpGet]
    public async Task<IActionResult> Enter(int applicationId)
    {
        ViewData["Title"] = "Enter Re-Exam Marks";
        var model = await _resultService.GetEntryFormAsync(applicationId);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Enter(int applicationId, decimal reExamMark)
    {
        var result = await _resultService.EnterMarksAsync(applicationId, reExamMark);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
            return RedirectToAction(nameof(Enter), new { applicationId });
        }

        TempData["StatusMessage"] = "Re-exam mark recorded. The result now needs to be verified before publication.";
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, "EnterMarks", "ReExamResult", null, HttpContext.Connection.RemoteIpAddress?.ToString(), $"ApplicationId={applicationId}, ReExamMark={reExamMark}");

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Review(bool? verified, bool? published, string? search, int page = 1)
    {
        ViewData["Title"] = "Verify &amp; Publish Results";
        ViewData["Verified"] = verified;
        ViewData["Published"] = published;
        ViewData["Search"] = search;
        return View(await _resultService.GetPagedAsync(verified, published, search, page, 10));
    }

    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Result Details";
        var model = await _resultService.GetDetailsAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var result = await _resultService.VerifyAsync(id, userId);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Result verified.";
            await _auditLog.LogAsync(userId, User.Identity?.Name, "Verify", "ReExamResult", id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), null);
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var result = await _resultService.PublishAsync(id, userId);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Result published. The student has been notified.";
            await _auditLog.LogAsync(userId, User.Identity?.Name, "Publish", "ReExamResult", id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), null);
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
