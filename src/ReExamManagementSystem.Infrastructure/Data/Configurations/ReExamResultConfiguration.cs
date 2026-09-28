using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class ReExamResultConfiguration : IEntityTypeConfiguration<ReExamResult>
{
    public void Configure(EntityTypeBuilder<ReExamResult> builder)
    {
        builder.Property(r => r.OriginalMark).HasColumnType("decimal(5,2)");
        builder.Property(r => r.ReExamMark).HasColumnType("decimal(5,2)");
        builder.Property(r => r.FinalMark).HasColumnType("decimal(5,2)");
        builder.Property(r => r.GradePoint).HasColumnType("decimal(4,2)");
        builder.Property(r => r.Grade).IsRequired().HasMaxLength(5);

        builder.HasIndex(r => r.ReExamApplicationId).IsUnique();

        builder.HasOne(r => r.ReExamApplication)
            .WithOne(a => a.ReExamResult)
            .HasForeignKey<ReExamResult>(r => r.ReExamApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Student)
            .WithMany(s => s.ReExamResults)
            .HasForeignKey(r => r.StudentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Course)
            .WithMany()
            .HasForeignKey(r => r.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
