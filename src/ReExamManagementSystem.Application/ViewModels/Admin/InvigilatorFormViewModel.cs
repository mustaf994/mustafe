using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class InvigilatorFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Staff ID is required.")]
    [StringLength(30)]
    [Display(Name = "Staff ID")]
    public string StaffId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    public InvigilatorStatus Status { get; set; } = InvigilatorStatus.Active;

    public List<SelectOption> DepartmentOptions { get; set; } = new();
}
