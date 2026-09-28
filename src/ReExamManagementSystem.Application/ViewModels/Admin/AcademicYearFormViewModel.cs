using System.ComponentModel.DataAnnotations;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class AcademicYearFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(20)]
    [Display(Name = "Academic Year (e.g. 2025/2026)")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}
