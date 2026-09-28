using System.ComponentModel.DataAnnotations;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class FacultyFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Faculty name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Faculty code is required.")]
    [StringLength(20)]
    [Display(Name = "Code")]
    public string Code { get; set; } = string.Empty;
}
