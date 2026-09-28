using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

public class Student : BaseEntity
{
    public string StudentNumber { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the ASP.NET Core Identity user (AspNetUsers.Id).
    /// Intentionally not a navigation property: ApplicationUser lives in the
    /// Infrastructure layer, which Domain must not reference. The relationship
    /// is configured with Fluent API in Infrastructure/Data/Configurations.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int ProgramId { get; set; }
    public AcademicProgram Program { get; set; } = null!;

    public int Level { get; set; }

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public int SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    public DateTime EnrollmentDate { get; set; }
    public StudentStatus Status { get; set; } = StudentStatus.Active;

    public ICollection<StudentResult> StudentResults { get; set; } = new List<StudentResult>();
    public ICollection<ReExamEligibility> ReExamEligibilities { get; set; } = new List<ReExamEligibility>();
    public ICollection<ReExamApplication> ReExamApplications { get; set; } = new List<ReExamApplication>();
    public ICollection<ExamAttendance> ExamAttendances { get; set; } = new List<ExamAttendance>();
    public ICollection<ReExamResult> ReExamResults { get; set; } = new List<ReExamResult>();
}
