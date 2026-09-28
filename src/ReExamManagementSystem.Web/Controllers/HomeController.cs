using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReExamManagementSystem.Web.Models;

namespace ReExamManagementSystem.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // There's no marketing landing page - the app opens straight into sign-in,
    // which itself redirects an already-authenticated user to their dashboard.
    public IActionResult Index()
    {
        return RedirectToAction("Login", "Account");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Single landing point for both unhandled exceptions (UseExceptionHandler)
    /// and non-2xx status codes (UseStatusCodePagesWithReExecute). Never
    /// exposes exception details or stack traces to the caller - only a
    /// request id they can hand to support, which is what actually gets logged.
    /// </summary>
    // Explicit route (rather than relying on the default {id?} convention route)
    // so UseStatusCodePagesWithReExecute("/Home/Error/{0}") always lands here
    // with the status code correctly bound, regardless of other routing changes.
    [Route("/Home/Error/{statusCode?}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int? statusCode = null)
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionFeature is not null)
        {
            _logger.LogError(exceptionFeature.Error, "Unhandled exception on {Path}. RequestId: {RequestId}", exceptionFeature.Path, requestId);
        }

        var model = new ErrorViewModel
        {
            RequestId = requestId,
            StatusCode = statusCode,
            Message = statusCode switch
            {
                404 => "The page you're looking for could not be found.",
                403 => "You do not have permission to view this page.",
                _ => "An unexpected error occurred while processing your request."
            }
        };

        Response.StatusCode = statusCode ?? 500;
        return View(model);
    }
}
