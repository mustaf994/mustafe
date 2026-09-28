using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class ReExamEligibilityConfiguration : IEntityTypeConfiguration<ReExamEligibility>
{
    public void Configure(EntityTypeBuilder<ReExamEligibility> builder)
    {
        builder.Property(e => e.Reason).HasMaxLength(500);

        builder.HasIndex(e => e.StudentResultId).IsUnique();

        builder.HasOne(e => e.Student)
            .WithMany(s => s.ReExamEligibilities)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.StudentResult)
            .WithOne(r => r.ReExamEligibility)
            .HasForeignKey<ReExamEligibility>(e => e.StudentResultId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Course)
            .WithMany(c => c.ReExamEligibilities)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.AcademicYear)
            .WithMany()
            .HasForeignKey(e => e.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Semester)
            .WithMany()
            .HasForeignKey(e => e.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
