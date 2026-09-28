using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class ExamAttendanceConfiguration : IEntityTypeConfiguration<ExamAttendance>
{
    public void Configure(EntityTypeBuilder<ExamAttendance> builder)
    {
        builder.HasIndex(a => new { a.ExamScheduleId, a.StudentId }).IsUnique();

        builder.HasOne(a => a.ExamSchedule)
            .WithMany(s => s.Attendances)
            .HasForeignKey(a => a.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Student)
            .WithMany(s => s.ExamAttendances)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
