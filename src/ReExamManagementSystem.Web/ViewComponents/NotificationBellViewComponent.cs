using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.ViewComponents;

public class NotificationBellViewComponent : ViewComponent
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationBellViewComponent(INotificationService notificationService, UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = _userManager.GetUserId(UserClaimsPrincipal);
        if (userId is null)
        {
            return Content(string.Empty);
        }

        var recent = await _notificationService.GetRecentAsync(userId);
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        ViewBag.UnreadCount = unreadCount;
        return View(recent);
    }
}
