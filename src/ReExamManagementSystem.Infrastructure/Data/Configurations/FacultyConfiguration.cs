using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.Property(f => f.Name).IsRequired().HasMaxLength(150);
        builder.Property(f => f.Code).IsRequired().HasMaxLength(20);

        builder.HasIndex(f => f.Code).IsUnique();

        builder.HasMany(f => f.Departments)
            .WithOne(d => d.Faculty)
            .HasForeignKey(d => d.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
