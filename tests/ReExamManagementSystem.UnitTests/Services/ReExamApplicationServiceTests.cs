using ReExamManagementSystem.Application.Services;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Infrastructure.Data;
using ReExamManagementSystem.Infrastructure.Repositories;
using ReExamManagementSystem.UnitTests.TestHelpers;

namespace ReExamManagementSystem.UnitTests.Services;

public class ReExamApplicationServiceTests
{
    private static async Task<(ApplicationDbContext Context, Student Student, ReExamEligibility Eligibility)> SeedEligibleStudentAsync(bool openForReExam = true)
    {
        var context = InMemoryDbContextFactory.Create();
        var (_, department, program, academicYear, semester, course) = await TestDataBuilder.SeedAcademicStructureAsync(context);
        var student = await TestDataBuilder.SeedStudentAsync(context, department, program, academicYear, semester);
        var result = await TestDataBuilder.SeedFailingResultAsync(context, student, course, academicYear, semester);

        var eligibility = new ReExamEligibility
        {
            StudentId = student.Id,
            StudentResultId = result.Id,
            CourseId = course.Id,
            AcademicYearId = academicYear.Id,
            SemesterId = semester.Id,
            Status = EligibilityStatus.Eligible,
            Reason = "Failed with mark below pass threshold."
        };
        context.ReExamEligibilities.Add(eligibility);

        if (openForReExam)
        {
            context.ReExamSubjects.Add(new ReExamSubject { CourseId = course.Id, AcademicYearId = academicYear.Id, SemesterId = semester.Id, IsActive = true });
        }

        await context.SaveChangesAsync();

        return (context, student, eligibility);
    }

    [Fact]
    public async Task SubmitAsync_Succeeds_WhenCourseIsOpenForReExam()
    {
        var (context, student, eligibility) = await SeedEligibleStudentAsync(openForReExam: true);
        using var _ = context;
        var service = new ReExamApplicationService(new UnitOfWork(context), new FakeNotificationService(), new FakeUserAccountService());

        var result = await service.SubmitAsync(student.Id, eligibility.Id);

        Assert.True(result.Succeeded);
        Assert.Single(context.ReExamApplications);
    }

    [Fact]
    public async Task SubmitAsync_Fails_WhenCourseIsNotOpenForReExam()
    {
        var (context, student, eligibility) = await SeedEligibleStudentAsync(openForReExam: false);
        using var _ = context;
        var service = new ReExamApplicationService(new UnitOfWork(context), new FakeNotificationService(), new FakeUserAccountService());

        var result = await service.SubmitAsync(student.Id, eligibility.Id);

        Assert.False(result.Succeeded);
        Assert.Empty(context.ReExamApplications);
    }

    [Fact]
    public async Task SubmitAsync_Fails_OnDuplicateApplicationForSameStudentCourseAndTerm()
    {
        var (context, student, eligibility) = await SeedEligibleStudentAsync(openForReExam: true);
        using var _ = context;
        var service = new ReExamApplicationService(new UnitOfWork(context), new FakeNotificationService(), new FakeUserAccountService());

        var first = await service.SubmitAsync(student.Id, eligibility.Id);
        var second = await service.SubmitAsync(student.Id, eligibility.Id);

        Assert.True(first.Succeeded);
        Assert.False(second.Succeeded);
        Assert.Single(context.ReExamApplications);
    }

    [Fact]
    public async Task ApproveAsync_NotifiesTheStudent()
    {
        var (context, student, eligibility) = await SeedEligibleStudentAsync(openForReExam: true);
        using var _ = context;
        var notifications = new FakeNotificationService();
        var service = new ReExamApplicationService(new UnitOfWork(context), notifications, new FakeUserAccountService());

        await service.SubmitAsync(student.Id, eligibility.Id);
        var application = Assert.Single(context.ReExamApplications);

        var approveResult = await service.ApproveAsync(application.Id, "officer-1");

        Assert.True(approveResult.Succeeded);
        Assert.Contains(notifications.Created, n => n.UserId == student.UserId && n.Type == NotificationType.ApplicationApproved);
    }

    [Fact]
    public async Task RejectAsync_RequiresAReason()
    {
        var (context, student, eligibility) = await SeedEligibleStudentAsync(openForReExam: true);
        using var _ = context;
        var service = new ReExamApplicationService(new UnitOfWork(context), new FakeNotificationService(), new FakeUserAccountService());

        await service.SubmitAsync(student.Id, eligibility.Id);
        var application = Assert.Single(context.ReExamApplications);

        var result = await service.RejectAsync(application.Id, "officer-1", reason: "");

        Assert.False(result.Succeeded);
    }
}
