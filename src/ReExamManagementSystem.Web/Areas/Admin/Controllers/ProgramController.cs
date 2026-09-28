using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class ProgramController : AdminControllerBase
{
    private readonly IProgramService _programService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProgramController(IProgramService programService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _programService = programService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Program Management";
        ViewData["Search"] = search;
        return View(await _programService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => PartialView("_Form", await _programService.GetForCreateAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProgramFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.DepartmentOptions = (await _programService.GetForCreateAsync()).DepartmentOptions;
            return PartialView("_Form", model);
        }

        var result = await _programService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            model.DepartmentOptions = (await _programService.GetForCreateAsync()).DepartmentOptions;
            return PartialView("_Form", model);
        }

        await LogAsync("Create", model.Name);
        TempData["StatusMessage"] = $"Program '{model.Name}' was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _programService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProgramFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            model.DepartmentOptions = (await _programService.GetForCreateAsync()).DepartmentOptions;
            return PartialView("_Form", model);
        }

        var result = await _programService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            model.DepartmentOptions = (await _programService.GetForCreateAsync()).DepartmentOptions;
            return PartialView("_Form", model);
        }

        await LogAsync("Update", model.Name);
        TempData["StatusMessage"] = $"Program '{model.Name}' was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _programService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Program was deleted.";
            await LogAsync("Delete", $"Program #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "AcademicProgram", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
