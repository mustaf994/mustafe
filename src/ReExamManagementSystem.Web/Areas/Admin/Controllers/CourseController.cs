using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class CourseController : AdminControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public CourseController(ICourseService courseService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _courseService = courseService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Course Management";
        ViewData["Search"] = search;
        return View(await _courseService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => PartialView("_Form", await _courseService.GetForCreateAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await RepopulateOptionsAsync(model);
            return PartialView("_Form", model);
        }

        var result = await _courseService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await RepopulateOptionsAsync(model);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", $"{model.Code} - {model.Name}");
        TempData["StatusMessage"] = $"Course '{model.Code}' was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _courseService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CourseFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await RepopulateOptionsAsync(model);
            return PartialView("_Form", model);
        }

        var result = await _courseService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await RepopulateOptionsAsync(model);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", $"{model.Code} - {model.Name}");
        TempData["StatusMessage"] = $"Course '{model.Code}' was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _courseService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Course was deleted.";
            await LogAsync("Delete", $"Course #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task RepopulateOptionsAsync(CourseFormViewModel model)
    {
        var fresh = await _courseService.GetForCreateAsync();
        model.DepartmentOptions = fresh.DepartmentOptions;
        model.AcademicYearOptions = fresh.AcademicYearOptions;
        model.SemesterOptions = fresh.SemesterOptions;
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "Course", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
