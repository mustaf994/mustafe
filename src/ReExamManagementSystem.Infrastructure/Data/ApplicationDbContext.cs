using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ReExamManagementSystem.Domain.Common;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Infrastructure.Data;

/// <summary>
/// EF Core Code First context. Inherits IdentityDbContext so the Identity
/// schema (users, roles, claims, logins, tokens) lives alongside the
/// application's domain tables in the same database.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<AcademicProgram> AcademicPrograms => Set<AcademicProgram>();
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Semester> Semesters => Set<Semester>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<StudentResult> StudentResults => Set<StudentResult>();
    public DbSet<ReExamEligibility> ReExamEligibilities => Set<ReExamEligibility>();
    public DbSet<ReExamSubject> ReExamSubjects => Set<ReExamSubject>();
    public DbSet<ReExamApplication> ReExamApplications => Set<ReExamApplication>();
    public DbSet<ExamRoom> ExamRooms => Set<ExamRoom>();
    public DbSet<Invigilator> Invigilators => Set<Invigilator>();
    public DbSet<ExamSchedule> ExamSchedules => Set<ExamSchedule>();
    public DbSet<ExamAttendance> ExamAttendances => Set<ExamAttendance>();
    public DbSet<ReExamResult> ReExamResults => Set<ReExamResult>();
    public DbSet<GradingRule> GradingRules => Set<GradingRule>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // The domain graph converges heavily on shared reference data (Student,
        // Course, AcademicYear, Semester...), which would make SQL Server reject
        // the default cascade-delete behavior with "may cause cycles or multiple
        // cascade paths". Every relationship *among our own entities* is restricted
        // here as the single source of truth; deletions of referenced rows must go
        // through the service layer (deactivate/soft-delete) rather than cascading.
        // Scoped to Domain.Entities only - Identity's own tables (AspNetUserRoles,
        // AspNetUserClaims, etc.) rely on their built-in cascade delete to remove a
        // user's role/claim/login/token rows when UserManager.DeleteAsync runs.
        const string domainEntitiesNamespace = "ReExamManagementSystem.Domain.Entities";
        foreach (var relationship in builder.Model.GetEntityTypes()
                     .Where(e => e.ClrType.Namespace == domainEntitiesNamespace)
                     .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    // Npgsql only writes UTC DateTimes to 'timestamp with time zone' columns, but
    // seed data and form input produce Kind=Unspecified/Local values. Normalize
    // every DateTime to UTC on the way in (and tag reads as UTC) so those writes
    // don't throw. Converters don't change column types, so no migration is needed.
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        configurationBuilder.Properties<DateTime?>().HaveConversion<NullableUtcDateTimeConverter>();
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private sealed class UtcDateTimeConverter()
        : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => ToUtc(v),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    private sealed class NullableUtcDateTimeConverter()
        : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? ToUtc(v.Value) : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
