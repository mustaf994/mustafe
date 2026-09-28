using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Student.Controllers;

public class DashboardController : StudentAreaControllerBase
{
    private readonly IStudentDashboardService _dashboardService;
    private readonly IStudentService _studentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(IStudentDashboardService dashboardService, IStudentService studentService, UserManager<ApplicationUser> userManager)
    {
        _dashboardService = dashboardService;
        _studentService = studentService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Student Dashboard";

        var userId = _userManager.GetUserId(User);
        if (userId is null) return Forbid();

        var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
        if (studentId is null) return Forbid();

        return View(await _dashboardService.GetAsync(studentId.Value, userId));
    }
}
