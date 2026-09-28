using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Infrastructure.Data;

namespace ReExamManagementSystem.UnitTests.TestHelpers;

/// <summary>Each call gets its own isolated in-memory database, so tests never see each other's data.</summary>
public static class InMemoryDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}
