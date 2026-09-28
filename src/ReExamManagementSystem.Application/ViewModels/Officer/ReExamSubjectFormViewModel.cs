using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Application.Common;

namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class ReExamSubjectFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a course.")]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Please select an academic year.")]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }

    [Required(ErrorMessage = "Please select a semester.")]
    [Display(Name = "Semester")]
    public int SemesterId { get; set; }

    public bool IsActive { get; set; } = true;

    public List<SelectOption> CourseOptions { get; set; } = new();
    public List<SelectOption> AcademicYearOptions { get; set; } = new();
    public List<SelectOption> SemesterOptions { get; set; } = new();
}
