using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Application.Common;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class DepartmentFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Department name is required.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department code is required.")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a faculty.")]
    [Display(Name = "Faculty")]
    public int FacultyId { get; set; }

    public List<SelectOption> FacultyOptions { get; set; } = new();
}
