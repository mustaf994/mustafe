using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;

namespace ReExamManagementSystem.Web.Areas.Admin.Controllers;

public class DashboardController : AdminControllerBase
{
    private readonly IAdminDashboardService _dashboardService;

    public DashboardController(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Administrator Dashboard";
        return View(await _dashboardService.GetAsync());
    }
}
