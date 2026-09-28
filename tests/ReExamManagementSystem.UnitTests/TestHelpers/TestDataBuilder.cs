using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Infrastructure.Data;

namespace ReExamManagementSystem.UnitTests.TestHelpers;

/// <summary>Builds the minimal, consistent reference data (faculty through course) that most service tests need, so each test file isn't re-deriving the same fixture graph.</summary>
public static class TestDataBuilder
{
    public static readonly (decimal Min, decimal Max, string Grade, decimal Point, bool Passing)[] DefaultGradingBands =
    {
        (90, 100, "A", 4.00m, true),
        (80, 89.99m, "B", 3.00m, true),
        (70, 79.99m, "C", 2.00m, true),
        (60, 69.99m, "D", 1.50m, true),
        (50, 59.99m, "E", 1.00m, true),
        (0, 49.99m, "F", 0.00m, false),
    };

    public static async Task<GradingRule[]> SeedGradingRulesAsync(ApplicationDbContext context)
    {
        var rules = DefaultGradingBands
            .Select(b => new GradingRule { MinMark = b.Min, MaxMark = b.Max, Grade = b.Grade, GradePoint = b.Point, IsPassing = b.Passing })
            .ToArray();

        context.GradingRules.AddRange(rules);
        await context.SaveChangesAsync();
        return rules;
    }

    public static async Task<(Faculty Faculty, Department Department, AcademicProgram Program, AcademicYear AcademicYear, Semester Semester, Course Course)>
        SeedAcademicStructureAsync(ApplicationDbContext context)
    {
        var faculty = new Faculty { Name = "Faculty of Science", Code = "SCI" };
        context.Faculties.Add(faculty);
        await context.SaveChangesAsync();

        var department = new Department { Name = "Computer Science", Code = "CS", FacultyId = faculty.Id };
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        var program = new AcademicProgram { Name = "BSc Computer Science", Code = "BSCCS", DurationYears = 4, DepartmentId = department.Id, Status = ProgramStatus.Active };
        context.AcademicPrograms.Add(program);

        var academicYear = new AcademicYear { Name = "2025/2026", StartDate = new DateTime(2025, 9, 1), EndDate = new DateTime(2026, 7, 31), IsActive = true };
        context.AcademicYears.Add(academicYear);
        await context.SaveChangesAsync();

        var semester = new Semester { Name = "Semester One", AcademicYearId = academicYear.Id, StartDate = academicYear.StartDate, EndDate = academicYear.StartDate.AddMonths(4), IsActive = true };
        context.Semesters.Add(semester);
        await context.SaveChangesAsync();

        var course = new Course { Code = "CS101", Name = "Introduction to Programming", CreditHours = 3, Level = 1, DepartmentId = department.Id, AcademicYearId = academicYear.Id, SemesterId = semester.Id };
        context.Courses.Add(course);
        await context.SaveChangesAsync();

        return (faculty, department, program, academicYear, semester, course);
    }

    public static async Task<Student> SeedStudentAsync(ApplicationDbContext context, Department department, AcademicProgram program, AcademicYear academicYear, Semester semester, string studentNumber = "CS/2022/001")
    {
        var student = new Student
        {
            StudentNumber = studentNumber,
            UserId = Guid.NewGuid().ToString(),
            FullName = "Test Student",
            Gender = Gender.Male,
            DateOfBirth = new DateTime(2002, 1, 1),
            PhoneNumber = "0000000000",
            Email = $"{studentNumber.Replace("/", "-")}@student.reexam.edu",
            DepartmentId = department.Id,
            ProgramId = program.Id,
            Level = 2,
            AcademicYearId = academicYear.Id,
            SemesterId = semester.Id,
            EnrollmentDate = new DateTime(2022, 9, 1),
            Status = StudentStatus.Active
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();
        return student;
    }

    public static async Task<StudentResult> SeedFailingResultAsync(ApplicationDbContext context, Student student, Course course, AcademicYear academicYear, Semester semester, decimal mark = 40)
    {
        var result = new StudentResult
        {
            StudentId = student.Id,
            CourseId = course.Id,
            AcademicYearId = academicYear.Id,
            SemesterId = semester.Id,
            Marks = mark,
            Grade = "F",
            GradePoint = 0,
            Status = ResultStatus.Fail
        };
        context.StudentResults.Add(result);
        await context.SaveChangesAsync();
        return result;
    }
}
