using ReExamManagementSystem.Application.Services;
using ReExamManagementSystem.Infrastructure.Repositories;
using ReExamManagementSystem.UnitTests.TestHelpers;

namespace ReExamManagementSystem.UnitTests.Services;

public class EligibilityServiceTests
{
    [Fact]
    public async Task RecomputeAsync_CreatesEligibilityForEachFailingResultOnce()
    {
        using var context = InMemoryDbContextFactory.Create();
        var (_, department, program, academicYear, semester, course) = await TestDataBuilder.SeedAcademicStructureAsync(context);
        var student = await TestDataBuilder.SeedStudentAsync(context, department, program, academicYear, semester);
        await TestDataBuilder.SeedFailingResultAsync(context, student, course, academicYear, semester);

        var service = new EligibilityService(new UnitOfWork(context));

        var firstRun = await service.RecomputeAsync();
        var secondRun = await service.RecomputeAsync();

        Assert.Equal(1, firstRun);
        Assert.Equal(0, secondRun); // idempotent: the same failing result must not produce a second record
        Assert.Single(context.ReExamEligibilities);
    }

    [Fact]
    public async Task RecomputeAsync_IgnoresPassingResults()
    {
        using var context = InMemoryDbContextFactory.Create();
        var (_, department, program, academicYear, semester, course) = await TestDataBuilder.SeedAcademicStructureAsync(context);
        var student = await TestDataBuilder.SeedStudentAsync(context, department, program, academicYear, semester);

        context.StudentResults.Add(new ReExamManagementSystem.Domain.Entities.StudentResult
        {
            StudentId = student.Id,
            CourseId = course.Id,
            AcademicYearId = academicYear.Id,
            SemesterId = semester.Id,
            Marks = 85,
            Grade = "B",
            GradePoint = 3.0m,
            Status = ReExamManagementSystem.Domain.Enums.ResultStatus.Pass
        });
        await context.SaveChangesAsync();

        var service = new EligibilityService(new UnitOfWork(context));
        var created = await service.RecomputeAsync();

        Assert.Equal(0, created);
        Assert.Empty(context.ReExamEligibilities);
    }
}
