using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class InvigilatorConfiguration : IEntityTypeConfiguration<Invigilator>
{
    public void Configure(EntityTypeBuilder<Invigilator> builder)
    {
        builder.Property(i => i.StaffId).IsRequired().HasMaxLength(30);
        builder.Property(i => i.FullName).IsRequired().HasMaxLength(150);
        builder.Property(i => i.Email).IsRequired().HasMaxLength(200);
        builder.Property(i => i.PhoneNumber).HasMaxLength(20);

        builder.HasIndex(i => i.StaffId).IsUnique();
        builder.HasIndex(i => i.Email).IsUnique();

        builder.HasOne(i => i.Department)
            .WithMany(d => d.Invigilators)
            .HasForeignKey(i => i.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
