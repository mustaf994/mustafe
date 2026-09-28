using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class ExamRoomController : AdminControllerBase
{
    private readonly IExamRoomService _examRoomService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExamRoomController(IExamRoomService examRoomService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _examRoomService = examRoomService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Examination Rooms";
        ViewData["Search"] = search;
        return View(await _examRoomService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public IActionResult Create() => PartialView("_Form", new ExamRoomFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExamRoomFormViewModel model)
    {
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _examRoomService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Create", $"{model.Building} {model.RoomNumber}");
        TempData["StatusMessage"] = "Exam room was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _examRoomService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ExamRoomFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid) return PartialView("_Form", model);

        var result = await _examRoomService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            return PartialView("_Form", model);
        }

        await LogAsync("Update", $"{model.Building} {model.RoomNumber}");
        TempData["StatusMessage"] = "Exam room was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _examRoomService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Exam room was deleted.";
            await LogAsync("Delete", $"ExamRoom #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "ExamRoom", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
