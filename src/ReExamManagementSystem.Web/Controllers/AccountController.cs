using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Application.ViewModels.Account;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IAuditLogService _auditLog;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IAuditLogService auditLog,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailSender = emailSender;
        _auditLog = auditLog;
        _logger = logger;
    }

    private string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToRoleDashboard();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            await _auditLog.LogAsync(null, model.Email, "LoginFailed", "ApplicationUser", null, ClientIp, "No account with this email.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "This account has been deactivated. Contact the administrator.");
            await _auditLog.LogAsync(user.Id, user.Email, "LoginFailed", "ApplicationUser", user.Id, ClientIp, "Account is deactivated.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            await _auditLog.LogAsync(user.Id, user.Email, "Login", "ApplicationUser", user.Id, ClientIp, "Successful login.");

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToRoleDashboard();
        }

        if (result.IsLockedOut)
        {
            await _auditLog.LogAsync(user.Id, user.Email, "LoginFailed", "ApplicationUser", user.Id, ClientIp, "Account locked out.");
            ModelState.AddModelError(string.Empty, "This account has been locked out due to multiple failed login attempts. Try again later.");
            return View(model);
        }

        await _auditLog.LogAsync(user.Id, user.Email, "LoginFailed", "ApplicationUser", user.Id, ClientIp, "Invalid password.");
        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userId = _userManager.GetUserId(User);
        var email = User.Identity?.Name;

        await _signInManager.SignOutAsync();
        await _auditLog.LogAsync(userId, email, "Logout", "ApplicationUser", userId, ClientIp, "User signed out.");

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        // Always redirect to the confirmation page, whether or not the account
        // exists, so the form can't be used to enumerate registered emails.
        if (user is not null && user.IsActive)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = Url.Action("ResetPassword", "Account", new { email = user.Email, token = encodedToken }, Request.Scheme)!;

            await _emailSender.SendEmailAsync(
                model.Email,
                "Reset your Re-Exam Management System password",
                $"Reset your password by <a href=\"{HtmlEncoder.Default.Encode(resetUrl)}\">clicking here</a>.");

            await _auditLog.LogAsync(user.Id, user.Email, "PasswordResetRequested", "ApplicationUser", user.Id, ClientIp, null);
        }

        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation() => View();

    [HttpGet]
    public IActionResult ResetPassword(string? email, string? token)
    {
        if (email is null || token is null)
        {
            return BadRequest("A password reset token and email must be supplied.");
        }

        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        // Without this, the "token" action-parameter binding leaves a ModelState
        // entry that asp-for="Token" picks up ahead of the decoded value below
        // (ModelState lookups are case-insensitive, so "token" collides with
        // "Token"), re-rendering the still-encoded value and breaking every
        // password reset with "Invalid token." Clearing it restores normal
        // asp-for -> model-property rendering.
        ModelState.Clear();
        return View(new ResetPasswordViewModel { Email = email, Token = decodedToken });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            // Don't reveal that the user does not exist.
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            await _auditLog.LogAsync(user.Id, user.Email, "PasswordReset", "ApplicationUser", user.Id, ClientIp, "Password reset via email token.");
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ResetPasswordConfirmation() => View();

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user);

        return View(new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Roles = roles,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            await _auditLog.LogAsync(user.Id, user.Email, "PasswordChanged", "ApplicationUser", user.Id, ClientIp, "User changed their own password.");
            TempData["StatusMessage"] = "Your password has been changed.";
            return RedirectToAction(nameof(Manage));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    private IActionResult RedirectToRoleDashboard()
    {
        if (User.IsInRole(Roles.Administrator))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
        }

        if (User.IsInRole(Roles.ExaminationOfficer))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Officer" });
        }

        if (User.IsInRole(Roles.Student))
        {
            return RedirectToAction("Index", "Dashboard", new { area = "Student" });
        }

        return RedirectToAction("Index", "Home");
    }
}
