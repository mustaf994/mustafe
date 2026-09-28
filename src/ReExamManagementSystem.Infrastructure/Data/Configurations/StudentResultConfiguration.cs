using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class StudentResultConfiguration : IEntityTypeConfiguration<StudentResult>
{
    public void Configure(EntityTypeBuilder<StudentResult> builder)
    {
        builder.Property(r => r.Marks).HasColumnType("decimal(5,2)");
        builder.Property(r => r.GradePoint).HasColumnType("decimal(4,2)");
        builder.Property(r => r.Grade).IsRequired().HasMaxLength(5);

        builder.HasIndex(r => new { r.StudentId, r.CourseId }).IsUnique();

        builder.HasOne(r => r.Student)
            .WithMany(s => s.StudentResults)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Course)
            .WithMany(c => c.StudentResults)
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AcademicYear)
            .WithMany()
            .HasForeignKey(r => r.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Semester)
            .WithMany()
            .HasForeignKey(r => r.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
