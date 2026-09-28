using ReExamManagementSystem.Application.Services;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Infrastructure.Data;
using ReExamManagementSystem.Infrastructure.Repositories;
using ReExamManagementSystem.UnitTests.TestHelpers;

namespace ReExamManagementSystem.UnitTests.Services;

public class ResultServiceTests
{
    private static async Task<(ApplicationDbContext Context, ReExamApplication Application, Student Student)> SeedApprovedApplicationAsync(decimal originalMark = 40)
    {
        var context = InMemoryDbContextFactory.Create();
        await TestDataBuilder.SeedGradingRulesAsync(context);
        var (_, department, program, academicYear, semester, course) = await TestDataBuilder.SeedAcademicStructureAsync(context);
        var student = await TestDataBuilder.SeedStudentAsync(context, department, program, academicYear, semester);
        var studentResult = await TestDataBuilder.SeedFailingResultAsync(context, student, course, academicYear, semester, originalMark);

        var eligibility = new ReExamEligibility
        {
            StudentId = student.Id,
            StudentResultId = studentResult.Id,
            CourseId = course.Id,
            AcademicYearId = academicYear.Id,
            SemesterId = semester.Id,
            Status = EligibilityStatus.Eligible,
            Reason = "Failed."
        };
        context.ReExamEligibilities.Add(eligibility);
        await context.SaveChangesAsync();

        var application = new ReExamApplication
        {
            StudentId = student.Id,
            CourseId = course.Id,
            ReExamEligibilityId = eligibility.Id,
            AcademicYearId = academicYear.Id,
            SemesterId = semester.Id,
            ApplicationDate = DateTime.UtcNow,
            Status = ApplicationStatus.Approved
        };
        context.ReExamApplications.Add(application);
        await context.SaveChangesAsync();

        return (context, application, student);
    }

    [Fact]
    public async Task EnterMarksAsync_ComputesFinalMarkGradeAndStatusFromTheReExamMark()
    {
        var (context, application, _) = await SeedApprovedApplicationAsync(originalMark: 40);
        using var _ = context;
        var service = new ResultService(new UnitOfWork(context), new GradeCalculationService(new UnitOfWork(context)), new FakeNotificationService());

        var result = await service.EnterMarksAsync(application.Id, reExamMark: 78);

        Assert.True(result.Succeeded);
        var stored = Assert.Single(context.ReExamResults);
        Assert.Equal(40, stored.OriginalMark);
        Assert.Equal(78, stored.ReExamMark);
        Assert.Equal(78, stored.FinalMark);
        Assert.Equal("C", stored.Grade);
        Assert.Equal(ResultStatus.Pass, stored.Status);
        Assert.False(stored.IsVerified);
        Assert.False(stored.IsPublished);
    }

    [Fact]
    public async Task PublishAsync_Fails_WhenResultHasNotBeenVerified()
    {
        var (context, application, _) = await SeedApprovedApplicationAsync();
        using var _ = context;
        var service = new ResultService(new UnitOfWork(context), new GradeCalculationService(new UnitOfWork(context)), new FakeNotificationService());
        await service.EnterMarksAsync(application.Id, reExamMark: 65);
        var result = Assert.Single(context.ReExamResults);

        var publishResult = await service.PublishAsync(result.Id, "officer-1");

        Assert.False(publishResult.Succeeded);
        Assert.False(context.ReExamResults.Single().IsPublished);
    }

    [Fact]
    public async Task PublishAsync_Succeeds_AndNotifiesTheStudent_OnceVerified()
    {
        var (context, application, student) = await SeedApprovedApplicationAsync();
        using var _ = context;
        var notifications = new FakeNotificationService();
        var service = new ResultService(new UnitOfWork(context), new GradeCalculationService(new UnitOfWork(context)), notifications);
        await service.EnterMarksAsync(application.Id, reExamMark: 65);
        var result = Assert.Single(context.ReExamResults);

        await service.VerifyAsync(result.Id, "officer-1");
        var publishResult = await service.PublishAsync(result.Id, "officer-1");

        Assert.True(publishResult.Succeeded);
        Assert.True(context.ReExamResults.Single().IsPublished);
        Assert.Contains(notifications.Created, n => n.UserId == student.UserId && n.Type == NotificationType.ResultPublished);
    }
}
