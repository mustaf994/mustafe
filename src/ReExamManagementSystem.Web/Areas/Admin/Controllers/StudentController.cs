using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Admin;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class StudentController : AdminControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentController(IStudentService studentService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _studentService = studentService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search, int? departmentId, int page = 1)
    {
        ViewData["Title"] = "Student Management";
        ViewData["Search"] = search;
        ViewData["DepartmentId"] = departmentId;
        ViewData["DepartmentOptions"] = (await _studentService.GetForCreateAsync()).DepartmentOptions;
        return View(await _studentService.GetPagedAsync(search, departmentId, page, 10));
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = await _studentService.GetDetailsAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewData["Title"] = "Add Student";
        return View(await _studentService.GetForCreateAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentFormViewModel model)
    {
        ViewData["Title"] = "Add Student";
        if (!ModelState.IsValid)
        {
            await ReloadOptionsAsync(model);
            return View(model);
        }

        var result = await _studentService.CreateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await ReloadOptionsAsync(model);
            return View(model);
        }

        await LogAsync("Create", $"{model.StudentNumber} - {model.FullName}");
        TempData["StatusMessage"] =
            $"Student '{model.FullName}' was created. Temporary password: {result.Data} " +
            "(share this with the student securely - they should change it after their first login).";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ViewData["Title"] = "Edit Student";
        var model = await _studentService.GetForEditAsync(id);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StudentFormViewModel model)
    {
        ViewData["Title"] = "Edit Student";
        model.Id = id;
        if (!ModelState.IsValid)
        {
            await ReloadOptionsAsync(model);
            return View(model);
        }

        var result = await _studentService.UpdateAsync(model);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error);
            await ReloadOptionsAsync(model);
            return View(model);
        }

        await LogAsync("Update", $"{model.StudentNumber} - {model.FullName}");
        TempData["StatusMessage"] = $"Student '{model.FullName}' was updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _studentService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Student was deleted.";
            await LogAsync("Delete", $"Student #{id}");
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task ReloadOptionsAsync(StudentFormViewModel model)
    {
        var fresh = await _studentService.GetForCreateAsync();
        model.DepartmentOptions = fresh.DepartmentOptions;
        model.ProgramOptions = fresh.ProgramOptions;
        model.AcademicYearOptions = fresh.AcademicYearOptions;
        model.SemesterOptions = fresh.SemesterOptions;
    }

    private async Task LogAsync(string action, string description)
    {
        var userId = _userManager.GetUserId(User);
        await _auditLog.LogAsync(userId, User.Identity?.Name, action, "Student", null, HttpContext.Connection.RemoteIpAddress?.ToString(), description);
    }
}
