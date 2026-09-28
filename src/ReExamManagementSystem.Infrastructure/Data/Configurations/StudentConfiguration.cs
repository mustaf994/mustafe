using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;
using ReExamManagementSystem.Infrastructure.Identity;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.Property(s => s.StudentNumber).IsRequired().HasMaxLength(30);
        builder.Property(s => s.FullName).IsRequired().HasMaxLength(150);
        builder.Property(s => s.PhoneNumber).HasMaxLength(20);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(200);
        builder.Property(s => s.UserId).IsRequired();

        builder.HasIndex(s => s.StudentNumber).IsUnique();
        builder.HasIndex(s => s.UserId).IsUnique();

        // FK to the Identity user without a CLR navigation property on Student,
        // keeping Domain free of any reference to ApplicationUser.
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Department)
            .WithMany(d => d.Students)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Program)
            .WithMany(p => p.Students)
            .HasForeignKey(s => s.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AcademicYear)
            .WithMany()
            .HasForeignKey(s => s.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Semester)
            .WithMany()
            .HasForeignKey(s => s.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
