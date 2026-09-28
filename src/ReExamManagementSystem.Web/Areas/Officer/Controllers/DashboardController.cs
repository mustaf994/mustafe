using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Application.Interfaces;

namespace ReExamManagementSystem.Web.Areas.Officer.Controllers;

public class DashboardController : OfficerControllerBase
{
    private readonly IOfficerDashboardService _dashboardService;

    public DashboardController(IOfficerDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Examination Officer Dashboard";
        return View(await _dashboardService.GetAsync());
    }
}
