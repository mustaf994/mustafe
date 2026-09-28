using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);
        builder.Property(d => d.Code).IsRequired().HasMaxLength(20);

        builder.HasIndex(d => d.Code).IsUnique();
    }
}
