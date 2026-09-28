using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReExamManagementSystem.Application.Interfaces;
using ReExamManagementSystem.Domain.Interfaces;
using ReExamManagementSystem.Infrastructure.Data;
using ReExamManagementSystem.Infrastructure.Repositories;
using ReExamManagementSystem.Infrastructure.Services;

namespace ReExamManagementSystem.Infrastructure;

/// <summary>
/// Composition root for the Infrastructure layer: the database context,
/// generic repository/unit-of-work, and (from later phases) concrete
/// implementations of Application-layer interfaces such as email or
/// notification senders.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IUserAccountService, UserAccountService>();
        services.AddScoped<IReportExportService, ReportExportService>();
        services.AddScoped<IStaffUserService, StaffUserService>();

        return services;
    }
}
