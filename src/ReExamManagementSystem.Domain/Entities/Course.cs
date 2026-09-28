using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

public class Course : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CreditHours { get; set; }
    public int Level { get; set; }
    public CourseStatus Status { get; set; } = CourseStatus.Active;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int SemesterId { get; set; }
    public Semester Semester { get; set; } = null!;

    public int AcademicYearId { get; set; }
    public AcademicYear AcademicYear { get; set; } = null!;

    public ICollection<StudentResult> StudentResults { get; set; } = new List<StudentResult>();
    public ICollection<ReExamEligibility> ReExamEligibilities { get; set; } = new List<ReExamEligibility>();
    public ICollection<ReExamSubject> ReExamSubjects { get; set; } = new List<ReExamSubject>();
    public ICollection<ReExamApplication> ReExamApplications { get; set; } = new List<ReExamApplication>();
    public ICollection<ExamSchedule> ExamSchedules { get; set; } = new List<ExamSchedule>();
}
