namespace ReExamManagementSystem.Application.Common;

/// <summary>
/// Canonical role names used throughout authorization policies, seeding and views.
/// Centralized here so a role name is never hard-coded as a magic string in a
/// controller or Razor view.
/// </summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string ExaminationOfficer = "ExaminationOfficer";
    public const string Student = "Student";

    public static readonly string[] All = { Administrator, ExaminationOfficer, Student };
}
