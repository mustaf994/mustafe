using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class ReExamApplicationConfiguration : IEntityTypeConfiguration<ReExamApplication>
{
    public void Configure(EntityTypeBuilder<ReExamApplication> builder)
    {
        builder.Property(a => a.RejectionReason).HasMaxLength(500);

        // Enforces "no duplicate ACTIVE application for the same student/course/term"
        // at the database level, in addition to the service-layer check. Filtered to
        // Pending (1), Approved (2) and Completed (5) so a student can re-apply after
        // Rejected (3) or Cancelled (4), matching ReExamApplicationService.SubmitAsync's
        // duplicate check - an unfiltered index previously threw on that legitimate
        // re-apply. SQL Server filtered index predicates don't support NOT/NOT IN, so
        // this lists the allowed statuses rather than excluding the other two.
        builder.HasIndex(a => new { a.StudentId, a.CourseId, a.AcademicYearId, a.SemesterId })
            .IsUnique()
            .HasFilter("[Status] IN (1, 2, 5)");

        builder.HasOne(a => a.Student)
            .WithMany(s => s.ReExamApplications)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Course)
            .WithMany(c => c.ReExamApplications)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ReExamEligibility)
            .WithMany(e => e.Applications)
            .HasForeignKey(a => a.ReExamEligibilityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AcademicYear)
            .WithMany()
            .HasForeignKey(a => a.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Semester)
            .WithMany()
            .HasForeignKey(a => a.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
