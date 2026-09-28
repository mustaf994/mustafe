using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class ExamRoomConfiguration : IEntityTypeConfiguration<ExamRoom>
{
    public void Configure(EntityTypeBuilder<ExamRoom> builder)
    {
        builder.Property(r => r.RoomNumber).IsRequired().HasMaxLength(20);
        builder.Property(r => r.Building).IsRequired().HasMaxLength(100);

        builder.HasIndex(r => new { r.Building, r.RoomNumber }).IsUnique();
    }
}
