using System.ComponentModel.DataAnnotations;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class StaffUserFormViewModel
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a role.")]
    public string Role { get; set; } = string.Empty;
}
