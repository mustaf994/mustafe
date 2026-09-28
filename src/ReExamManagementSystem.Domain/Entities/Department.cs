using ReExamManagementSystem.Domain.Common;

namespace ReExamManagementSystem.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public int FacultyId { get; set; }
    public Faculty Faculty { get; set; } = null!;

    public ICollection<AcademicProgram> Programs { get; set; } = new List<AcademicProgram>();
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Invigilator> Invigilators { get; set; } = new List<Invigilator>();
}
