using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class StudentFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Student number is required.")]
    [StringLength(30)]
    [Display(Name = "Student Number")]
    public string StudentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(150)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    public Gender Gender { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; } = DateTime.UtcNow.AddYears(-20);

    [Phone]
    [Display(Name = "Phone Number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a department.")]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Required(ErrorMessage = "Please select a program.")]
    [Display(Name = "Program")]
    public int ProgramId { get; set; }

    [Required]
    [Range(1, 8)]
    public int Level { get; set; } = 1;

    [Required(ErrorMessage = "Please select an academic year.")]
    [Display(Name = "Academic Year")]
    public int AcademicYearId { get; set; }

    [Required(ErrorMessage = "Please select a semester.")]
    [Display(Name = "Semester")]
    public int SemesterId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Enrollment Date")]
    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public List<SelectOption> DepartmentOptions { get; set; } = new();
    public List<ProgramOption> ProgramOptions { get; set; } = new();
    public List<SelectOption> AcademicYearOptions { get; set; } = new();
    public List<SemesterOption> SemesterOptions { get; set; } = new();
}
