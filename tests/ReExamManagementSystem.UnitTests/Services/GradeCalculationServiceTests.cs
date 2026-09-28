using ReExamManagementSystem.Application.Services;
using ReExamManagementSystem.Domain.Enums;
using ReExamManagementSystem.Infrastructure.Repositories;
using ReExamManagementSystem.UnitTests.TestHelpers;

namespace ReExamManagementSystem.UnitTests.Services;

public class GradeCalculationServiceTests
{
    [Theory]
    [InlineData(95, "A", 4.00, ResultStatus.Pass)]
    [InlineData(90, "A", 4.00, ResultStatus.Pass)]
    [InlineData(89.99, "B", 3.00, ResultStatus.Pass)]
    [InlineData(75, "C", 2.00, ResultStatus.Pass)]
    [InlineData(50, "E", 1.00, ResultStatus.Pass)]
    [InlineData(49.99, "F", 0.00, ResultStatus.Fail)]
    [InlineData(0, "F", 0.00, ResultStatus.Fail)]
    public async Task CalculateAsync_ReturnsTheConfiguredGradeBand(decimal mark, string expectedGrade, decimal expectedPoint, ResultStatus expectedStatus)
    {
        using var context = InMemoryDbContextFactory.Create();
        await TestDataBuilder.SeedGradingRulesAsync(context);
        var service = new GradeCalculationService(new UnitOfWork(context));

        var (grade, point, status) = await service.CalculateAsync(mark);

        Assert.Equal(expectedGrade, grade);
        Assert.Equal(expectedPoint, point);
        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public async Task CalculateAsync_ThrowsWhenNoRuleCoversTheMark()
    {
        using var context = InMemoryDbContextFactory.Create();
        // No grading rules seeded at all.
        var service = new GradeCalculationService(new UnitOfWork(context));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CalculateAsync(75));
    }
}
