using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class AcademicYearController : AdminControllerBase
{
    private readonly IAcademicYearService _academicYearService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public AcademicYearController(IAcademicYearService academicYearService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _academicYearService = academicYearService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Academic Year Management";
        ViewData["Search"] = search;
        return View(await _academicYearService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public IActionResult Create() => PartialView("_Form", new AcademicYearFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AcademicYearFormViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _academicYearService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", model.Name);
        TempData["StatusMessage"] = $"Academic year '{model.Name}' was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _academicYearService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AcademicYearFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _academicYearService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", model.Name);
        TempData["StatusMessage"] = $"Academic year '{model.Name}' was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _academicYearService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Academic year was deleted.";
            await LogAsync("Delete", $"AcademicYear #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "AcademicYear", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
