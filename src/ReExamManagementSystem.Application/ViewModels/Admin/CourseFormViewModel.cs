using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class CourseFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Course code is required.")]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Course name is required.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 12, ErrorMessage = "Credit hours must be between 1 and 12.")]
    [Display(Name = "Credit Hours")]
    public int CreditHours { get; set; } = 3;

    [Required]
    [Range(1, 8, ErrorMessage = "Level must be between 1 and 8.")]
    public int Level { get; set; } = 1;

    public CourseStatus Status { get; set; } = CourseStatus.Active;

    [Required(ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Please select an academic year.")]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }

    [Required(ErrorMessage = "Please select a semester.")]
    [Display(Name = "Semester")]
    public int SemesterId { get; set; }

    public List<SelectOption> DepartmentOptions { get; set; } = new();
    public List<SelectOption> AcademicYearOptions { get; set; } = new();
    public List<SelectOption> SemesterOptions { get; set; } = new();
}
