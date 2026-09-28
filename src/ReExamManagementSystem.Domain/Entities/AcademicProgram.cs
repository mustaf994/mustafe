using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Enums;

namespace ReExamManagementSystem.Domain.Entities;

/// <summary>
/// Represents a university academic program (the spec's "Program" table).
/// Named AcademicProgram in code to avoid colliding with the reader's mental
/// model of the top-level Program.cs entry point in the Web project.
/// </summary>
public class AcademicProgram : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int DurationYears { get; set; }
    public ProgramStatus Status { get; set; } = ProgramStatus.Active;

    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public ICollection<Student> Students { get; set; } = new List<Student>();
}
