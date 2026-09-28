using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class SystemSettingController : AdminControllerBase
{
    private readonly ISystemSettingService _settingService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public SystemSettingController(ISystemSettingService settingService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _settingService = settingService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "System Settings";
        return View(await _settingService.GetAllAsync());
    }

    [HttpGet]
    public IActionResult Create() => PartialView("_Form", new SystemSettingFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SystemSettingFormViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _settingService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", model.Key);
        TempData["StatusMessage"] = "Setting was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _settingService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SystemSettingFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _settingService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", model.Key);
        TempData["StatusMessage"] = "Setting was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _settingService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Setting was deleted.";
            await LogAsync("Delete", $"SystemSetting #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "SystemSetting", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
