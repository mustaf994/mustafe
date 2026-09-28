using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class InvigilatorController : AdminControllerBase
{
    private readonly IInvigilatorService _invigilatorService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public InvigilatorController(IInvigilatorService invigilatorService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _invigilatorService = invigilatorService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Invigilators";
        ViewData["Search"] = search;
        return View(await _invigilatorService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => PartialView("_Form", await _invigilatorService.GetForCreateAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvigilatorFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        var result = await _invigilatorService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", $"{model.StaffId} - {model.FullName}");
        TempData["StatusMessage"] = "Invigilator was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _invigilatorService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InvigilatorFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        var result = await _invigilatorService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", $"{model.StaffId} - {model.FullName}");
        TempData["StatusMessage"] = "Invigilator was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _invigilatorService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Invigilator was deleted.";
            await LogAsync("Delete", $"Invigilator #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ReloadOptionsAsync(InvigilatorFormViewModel model)
    {
        model.DepartmentOptions = (await _invigilatorService.GetForCreateAsync()).DepartmentOptions;
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "Invigilator", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
