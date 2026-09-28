using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Officer.Controllers;

public class EligibilityController : OfficerControllerBase
{
    private readonly IEligibilityService _eligibilityService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public EligibilityController(IEligibilityService eligibilityService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _eligibilityService = eligibilityService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Eligible Students";
        ViewData["Search"] = search;
        return View(await _eligibilityService.GetPagedAsync(search, page, 10));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Recompute()
    {
        var created = await _eligibilityService.RecomputeAsync();
        TempData["StatusMessage"] = created == 0
            ? "No new eligible students found - everything is already up to date."
            : $"{created} new eligibility record(s) created.";

        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, "Recompute", "ReExamEligibility", null, HttpContext.Connection.RemoteIpAddress?.ToString(), $"{created} new record(s) created.");

        return RedirectToAction(nameof(Index));
    }
}
