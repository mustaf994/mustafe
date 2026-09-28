using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Officer.Controllers;

public class ReExamSubjectController : OfficerControllerBase
{
    private readonly IReExamSubjectService _subjectService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public ReExamSubjectController(IReExamSubjectService subjectService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _subjectService = subjectService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Re-Exam Subjects";
        ViewData["Search"] = search;
        return View(await _subjectService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => PartialView("_Form", await _subjectService.GetForCreateAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReExamSubjectFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        var result = await _subjectService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", $"ReExamSubject CourseId={model.CourseId}");
        TempData["StatusMessage"] = "Re-exam subject was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _subjectService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReExamSubjectFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        var result = await _subjectService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", $"ReExamSubject #{id}");
        TempData["StatusMessage"] = "Re-exam subject was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _subjectService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Re-exam subject was removed.";
            await LogAsync("Delete", $"ReExamSubject #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ReloadOptionsAsync(ReExamSubjectFormViewModel model)
    {
        var fresh = await _subjectService.GetForCreateAsync();
        model.CourseOptions = fresh.CourseOptions;
        model.AcademicYearOptions = fresh.AcademicYearOptions;
        model.SemesterOptions = fresh.SemesterOptions;
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "ReExamSubject", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
