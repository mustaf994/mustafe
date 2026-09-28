using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class ProgramFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Program name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Program code is required.")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Range(1, 8, ErrorMessage = "Duration must be between 1 and 8 years.")]
    [Display(Name = "Duration (years)")]
    public int DurationYears { get; set; } = 4;

    public ProgramStatus Status { get; set; } = ProgramStatus.Active;

    [Required(ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    public List<SelectOption> DepartmentOptions { get; set; } = new();
}
