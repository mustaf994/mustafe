using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Officer;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Officer.Controllers;

public class ExamScheduleController : OfficerControllerBase
{
    private readonly IExamScheduleService _scheduleService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExamScheduleController(IExamScheduleService scheduleService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _scheduleService = scheduleService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Examination Schedule";
        ViewData["Search"] = search;
        return View(await _scheduleService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => PartialView("_Form", await _scheduleService.GetForCreateAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExamScheduleFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        var result = await _scheduleService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", $"ExamSchedule CourseId={model.CourseId} on {model.ExamDate:yyyy-MM-dd}");
        TempData["StatusMessage"] = "Exam schedule was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _scheduleService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExamScheduleFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        var result = await _scheduleService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await ReloadOptionsAsync(model);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", $"ExamSchedule #{id}");
        TempData["StatusMessage"] = "Exam schedule was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _scheduleService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Exam schedule was deleted.";
            await LogAsync("Delete", $"ExamSchedule #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ReloadOptionsAsync(ExamScheduleFormViewModel model)
    {
        var fresh = await _scheduleService.GetForCreateAsync();
        model.CourseOptions = fresh.CourseOptions;
        model.AcademicYearOptions = fresh.AcademicYearOptions;
        model.SemesterOptions = fresh.SemesterOptions;
        model.ExamRoomOptions = fresh.ExamRoomOptions;
        model.InvigilatorOptions = fresh.InvigilatorOptions;
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "ExamSchedule", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
