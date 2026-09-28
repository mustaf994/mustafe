using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);

        builder.HasIndex(c => new { c.Code, c.AcademicYearId, c.SemesterId }).IsUnique();

        builder.HasOne(c => c.Department)
            .WithMany(d => d.Courses)
            .HasForeignKey(c => c.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Semester)
            .WithMany()
            .HasForeignKey(c => c.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.AcademicYear)
            .WithMany()
            .HasForeignKey(c => c.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
