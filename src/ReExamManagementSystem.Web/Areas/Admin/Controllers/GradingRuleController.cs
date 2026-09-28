using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class GradingRuleController : AdminControllerBase
{
    private readonly IGradingRuleService _gradingRuleService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public GradingRuleController(IGradingRuleService gradingRuleService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _gradingRuleService = gradingRuleService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Grading Rules";
        return View(await _gradingRuleService.GetAllAsync());
    }

    [HttpGet]
    public IActionResult Create() => PartialView("_Form", new GradingRuleFormViewModel { IsPassing = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GradingRuleFormViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _gradingRuleService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", $"{model.Grade} ({model.MinMark}-{model.MaxMark})");
        TempData["StatusMessage"] = "Grading rule was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _gradingRuleService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GradingRuleFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _gradingRuleService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", $"{model.Grade} ({model.MinMark}-{model.MaxMark})");
        TempData["StatusMessage"] = "Grading rule was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _gradingRuleService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Grading rule was deleted.";
            await LogAsync("Delete", $"GradingRule #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "GradingRule", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
