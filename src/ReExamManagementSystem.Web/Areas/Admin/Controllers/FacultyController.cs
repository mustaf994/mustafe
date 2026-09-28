using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class FacultyController : AdminControllerBase
{
    private readonly IFacultyService _facultyService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public FacultyController(IFacultyService facultyService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _facultyService = facultyService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Faculty Management";
        ViewData["Search"] = search;
        var result = await _facultyService.GetPagedAsync(search, page, 10);
        return View(result);
    }

    [HttpGet]
    public IActionResult Create() => PartialView("_Form", new FacultyFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FacultyFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_Form", model);
        }

        var result = await _facultyService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", model.Name);
        TempData["StatusMessage"] = $"Faculty '{model.Name}' was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _facultyService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FacultyFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            return PartialView("_Form", model);
        }

        var result = await _facultyService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", model.Name);
        TempData["StatusMessage"] = $"Faculty '{model.Name}' was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _facultyService.DeleteAsync(id);
        TempData["StatusMessage"] = result.Succeeded
            ? "Faculty was deleted."
            : null;
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            await LogAsync("Delete", $"Faculty #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "Faculty", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
