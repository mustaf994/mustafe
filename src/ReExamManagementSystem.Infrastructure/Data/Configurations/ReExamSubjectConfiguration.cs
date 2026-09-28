using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class ReExamSubjectConfiguration : IEntityTypeConfiguration<ReExamSubject>
{
    public void Configure(EntityTypeBuilder<ReExamSubject> builder)
    {
        builder.HasIndex(s => new { s.CourseId, s.AcademicYearId, s.SemesterId }).IsUnique();

        builder.HasOne(s => s.Course)
            .WithMany(c => c.ReExamSubjects)
            .HasForeignKey(s => s.CourseId)
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
