using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class DepartmentController : AdminControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public DepartmentController(IDepartmentService departmentService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _departmentService = departmentService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        ViewData["Title"] = "Department Management";
        ViewData["Search"] = search;
        return View(await _departmentService.GetPagedAsync(search, page, 10));
    }

    [HttpGet]
    public async Task<IActionResult> Create() => PartialView("_Form", await _departmentService.GetForCreateAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.FacultyOptions = (await _departmentService.GetForCreateAsync()).FacultyOptions;
            return PartialView("_Form", model);
        }

        var result = await _departmentService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            model.FacultyOptions = (await _departmentService.GetForCreateAsync()).FacultyOptions;
            return PartialView("_Form", model);
        }

        await LogAsync("Create", model.Name);
        TempData["StatusMessage"] = $"Department '{model.Name}' was created.";
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _departmentService.GetForEditAsync(id);
        return model is null ? NotFound() : PartialView("_Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepartmentFormViewModel model)
    {
        model.Id = id;
        if (!ModelState.IsValid)
        {
            model.FacultyOptions = (await _departmentService.GetForCreateAsync()).FacultyOptions;
            return PartialView("_Form", model);
        }

        var result = await _departmentService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            model.FacultyOptions = (await _departmentService.GetForCreateAsync()).FacultyOptions;
            return PartialView("_Form", model);
        }

        await LogAsync("Update", model.Name);
        TempData["StatusMessage"] = $"Department '{model.Name}' was updated.";
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _departmentService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Department was deleted.";
            await LogAsync("Delete", $"Department #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "Department", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
