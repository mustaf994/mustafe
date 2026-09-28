using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Student.Controllers;

/// <summary>Self-service academic profile (department, program, level, enrollment) and result history, as opposed to Account/Manage which only shows generic Identity account info.</summary>
public class ProfileController : StudentAreaControllerBase
{
    private readonly IStudentService _studentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(IStudentService studentService, UserManager<ApplicationUser> userManager)
    {
        _studentService = studentService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Profile";

        var userId = _userManager.GetUserId(User);
        if (userId is null) return Forbid();

        var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
        if (studentId is null) return Forbid();

        var model = await _studentService.GetDetailsAsync(studentId.Value);
        return model is null ? NotFound() : View(model);
    }
}
