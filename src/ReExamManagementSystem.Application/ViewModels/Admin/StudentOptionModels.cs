using ReExamManagementSystem.Application.Common;

namespace ReExamManagementSystem.Application.ViewModels.Admin;

/// <summary>A Program dropdown option tagged with its Department, so the view can filter it client-side as the Department selection changes.</summary>
public class ProgramOption : SelectOption
{
    public int DepartmentId { get; set; }
}

/// <summary>A Semester dropdown option tagged with its AcademicYear, so the view can filter it client-side.</summary>
public class SemesterOption : SelectOption
{
    public int AcademicYearId { get; set; }
}
