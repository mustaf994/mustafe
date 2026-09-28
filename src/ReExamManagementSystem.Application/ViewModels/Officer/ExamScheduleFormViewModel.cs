using System.ComponentModel.DataAnnotations;
using ReExamManagementSystem.Application.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Officer;

public class ExamScheduleFormViewModel
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

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Exam Date")]
    public DateTime ExamDate { get; set; } = DateTime.UtcNow.Date.AddDays(7);

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Start Time")]
    public TimeSpan StartTime { get; set; } = new(9, 0, 0);

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "End Time")]
    public TimeSpan EndTime { get; set; } = new(11, 0, 0);

    [Required(ErrorMessage = "Please select an exam room.")]
    [Display(Name = "Exam Room")]
    public int ExamRoomId { get; set; }

    [Required(ErrorMessage = "Please select an invigilator.")]
    [Display(Name = "Invigilator")]
    public int InvigilatorId { get; set; }

    public ExamStatus Status { get; set; } = ExamStatus.Scheduled;

    public List<SelectOption> CourseOptions { get; set; } = new();
    public List<SelectOption> AcademicYearOptions { get; set; } = new();
    public List<SelectOption> SemesterOptions { get; set; } = new();
    public List<SelectOption> ExamRoomOptions { get; set; } = new();
    public List<SelectOption> InvigilatorOptions { get; set; } = new();
}
