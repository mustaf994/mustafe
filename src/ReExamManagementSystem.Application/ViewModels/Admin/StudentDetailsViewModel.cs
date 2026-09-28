using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

public class StudentDetailsViewModel
{
    public int Id { get; set; }
    public string StudentNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public int Level { get; set; }
    public string AcademicYearName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public StudentStatus Status { get; set; }

    public List<StudentResultRowViewModel> Results { get; set; } = new();
}

public class StudentResultRowViewModel
{
    public string CourseCode { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string AcademicYearName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public decimal Marks { get; set; }
    public string Grade { get; set; } = string.Empty;
    public decimal GradePoint { get; set; }
    public ResultStatus Status { get; set; }
}
