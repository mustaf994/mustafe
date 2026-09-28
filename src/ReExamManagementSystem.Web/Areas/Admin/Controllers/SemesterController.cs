using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class SemesterController : AdminControllerBase
{
    private readonly ISemesterService _semesterService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public SemesterController(ISemesterService semesterService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _semesterService = semesterService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Semester Management";
        ViewData["Search"] = search;
        return View(await _semesterService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => PartialView("_Form", await _semesterService.GetForCreateAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SemesterFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AcademicYearOptions = (await _semesterService.GetForCreateAsync()).AcademicYearOptions;
            return PartialView("_Form", model);
        }

        var result = await _semesterService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            model.AcademicYearOptions = (await _semesterService.GetForCreateAsync()).AcademicYearOptions;
            return PartialView("_Form", model);
        }

        await LogAsync("Create", model.Name);
        TempData["StatusMessage"] = $"Semester '{model.Name}' was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _semesterService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SemesterFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            model.AcademicYearOptions = (await _semesterService.GetForCreateAsync()).AcademicYearOptions;
            return PartialView("_Form", model);
        }

        var result = await _semesterService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            model.AcademicYearOptions = (await _semesterService.GetForCreateAsync()).AcademicYearOptions;
            return PartialView("_Form", model);
        }

        await LogAsync("Update", model.Name);
        TempData["StatusMessage"] = $"Semester '{model.Name}' was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _semesterService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Semester was deleted.";
            await LogAsync("Delete", $"Semester #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "Semester", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
