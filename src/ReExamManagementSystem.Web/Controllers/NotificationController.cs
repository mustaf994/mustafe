using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationController(INotificationService notificationService, UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Notifications";
        var userId = _userManager.GetUserId(User)!;
        return View(await _notificationService.GetAllAsync(userId));
    }

    public async Task<IActionResult> Open(int id)
    {
        var userId = _userManager.GetUserId(User)!;
        await _notificationService.MarkAsReadAsync(id, userId);

        var notification = (await _notificationService.GetAllAsync(userId)).FirstOrDefault(n => n.Id == id);
        if (notification?.Link is not null)
        {
            return LocalRedirect(notification.Link);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _userManager.GetUserId(User)!;
        await _notificationService.MarkAllAsReadAsync(userId);
        return RedirectToAction(nameof(Index));
    }
}
