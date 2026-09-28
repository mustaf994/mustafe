using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Application.Common;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class SemesterFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50)]
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

    [Required(ErrorMessage = "Please select an academic year.")]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }

    public List<SelectOption> AcademicYearOptions { get; set; } = new();
}
