using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

// Staff account management is the one Administrator capability the
// Examination Officer must not have, so this narrows the base class's
// "RequireStaff" policy back down to Administrator only. ASP.NET Core
// combines multiple Authorize attributes with AND semantics, so both
// policies must be satisfied - which only Administrator can do.
[Authorize(Policy = "RequireAdministrator")]
public class UserController : AdminControllerBase
{
    private readonly IStaffUserService _staffUserService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(IStaffUserService staffUserService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _staffUserService = staffUserService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "User Management";
        return View(await _staffUserService.GetAllAsync());
    }

    [HttpGet]
    public IActionResult Create() => PartialView("_Form", new StaffUserFormViewModel { Role = Roles.ExaminationOfficer });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffUserFormViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _staffUserService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        var actorId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(actorId, User.Identity?.Name, "Create", "ApplicationUser", null, HttpContext.Connection.RemoteIpAddress?.ToString(), $"{model.Role} account for {model.Email}");

        TempData["StatusMessage"] =
            $"Account created for '{model.FullName}' ({model.Role}). Temporary password: {result.Data} " +
            "(share this with them securely - they should change it after their first login).";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id, bool isActive)
    {
        var result = await _staffUserService.SetActiveAsync(id, isActive);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = isActive ? "Account activated." : "Account deactivated.";
            var actorId = _userManager.GetUserId(User);
            await _auditLog.LogAsync(actorId, User.Identity?.Name, isActive ? "Activate" : "Deactivate", "ApplicationUser", id, HttpContext.Connection.RemoteIpAddress?.ToString(), null);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _staffUserService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Account deleted.";
            await _auditLog.LogAsync(currentUserId, User.Identity?.Name, "Delete", "ApplicationUser", id, HttpContext.Connection.RemoteIpAddress?.ToString(), null);
        }

        return RedirectToAction(nameof(Index));
    }
}
