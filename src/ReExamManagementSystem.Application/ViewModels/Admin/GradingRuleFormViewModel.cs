using System.ComponentModel.DataAnnotations;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class GradingRuleFormViewModel
{
    public int Id { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Minimum mark must be between 0 and 100.")]
    [Display(Name = "Minimum Mark")]
    public decimal MinMark { get; set; }

    [Required]
    [Range(0, 100, ErrorMessage = "Maximum mark must be between 0 and 100.")]
    [Display(Name = "Maximum Mark")]
    public decimal MaxMark { get; set; }

    [Required(ErrorMessage = "Grade is required.")]
    [StringLength(5)]
    public string Grade { get; set; } = string.Empty;

    [Required]
    [Range(0, 5, ErrorMessage = "Grade point must be between 0 and 5.")]
    [Display(Name = "Grade Point")]
    public decimal GradePoint { get; set; }

    [Display(Name = "Counts as Passing")]
    public bool IsPassing { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }
}
