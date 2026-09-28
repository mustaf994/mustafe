using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Student.Controllers;

public class ResultController : StudentAreaControllerBase
{
    private readonly IStudentResultService _studentResultService;
    private readonly IStudentService _studentService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ResultController(IStudentResultService studentResultService, IStudentService studentService, UserManager<ApplicationUser> userManager)
    {
        _studentResultService = studentResultService;
        _studentService = studentService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "My Results";

        var userId = _userManager.GetUserId(User);
        if (userId is null) return Forbid();

        var studentId = await _studentService.GetStudentIdByUserIdAsync(userId);
        if (studentId is null) return Forbid();

        return View(await _studentResultService.GetPublishedResultsAsync(studentId.Value));
    }
}
