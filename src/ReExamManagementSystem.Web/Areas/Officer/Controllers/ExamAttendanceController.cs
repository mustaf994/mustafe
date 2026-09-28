using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Areas.Officer.Controllers;

public class ExamAttendanceController : OfficerControllerBase
{
    private readonly IExamAttendanceService _attendanceService;
    private readonly IAuditLogService _auditLog;
    private readonly UserManager<ApplicationUser> _userManager;

    public ExamAttendanceController(IExamAttendanceService attendanceService, IAuditLogService auditLog, UserManager<ApplicationUser> userManager)
    {
        _attendanceService = attendanceService;
        _auditLog = auditLog;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int scheduleId)
    {
        ViewData["Title"] = "Examination Attendance";
        var model = await _attendanceService.GetForScheduleAsync(scheduleId);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int scheduleId, Dictionary<int, AttendanceStatus> attendance)
    {
        var userId = _userManager.GetUserId(User)!;
        var result = await _attendanceService.SaveAsync(scheduleId, attendance, userId);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);
        }
        else
        {
            TempData["StatusMessage"] = "Attendance was recorded.";
            await _auditLog.LogAsync(userId, User.Identity?.Name, "RecordAttendance", "ExamAttendance", scheduleId.ToString(), HttpContext.Connection.RemoteIpAddress?.ToString(), $"{attendance.Count} record(s).");
        }

        return RedirectToAction(nameof(Index), new { scheduleId });
    }
}
