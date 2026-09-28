using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Officer.Controllers;

public class ApplicationController : OfficerControllerBase
{
    private readonly IReExamApplicationService _applicationService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationController(IReExamApplicationService applicationService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _applicationService = applicationService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(ApplicationStatus? status, string? search, int page = 1)
    {
        ViewData["Title"] = "Re-Exam Applications";
        ViewData["Status"] = status;
        ViewData["Search"] = search;
        return View(await _applicationService.GetPagedForReviewAsync(status, search, page, 10));
    }

    public async Task<IActionResult> Review(int id)
    {
        ViewData["Title"] = "Review Application";
        var model = await _applicationService.GetReviewDetailsAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        var result = await _applicationService.ApproveAsync(id, userId);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Application approved. The student has been notified.";
            await _auditLog.LogAsync(userId, User.Identity?.Name, "Approve", "ReExamApplication", id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), null);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string rejectionReason)
    {
        var userId = _userManager.GetUserId(User)!;
        var result = await _applicationService.RejectAsync(id, userId, rejectionReason);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Application rejected. The student has been notified.";
            await _auditLog.LogAsync(userId, User.Identity?.Name, "Reject", "ReExamApplication", id.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), rejectionReason);
        }

        return RedirectToAction(nameof(Index));
    }
}
