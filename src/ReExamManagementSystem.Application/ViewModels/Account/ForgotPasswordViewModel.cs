using System.ComponentModel.DataAnnotations;

namespace ReExamManagementSystem.Application.ViewModels.Account;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
