using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class SemesterConfiguration : IEntityTypeConfiguration<Semester>
{
    public void Configure(EntityTypeBuilder<Semester> builder)
    {
        builder.Property(s => s.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(s => new { s.AcademicYearId, s.Name }).IsUnique();
    }
}
