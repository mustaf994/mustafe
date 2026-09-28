using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class ExamScheduleConfiguration : IEntityTypeConfiguration<ExamSchedule>
{
    public void Configure(EntityTypeBuilder<ExamSchedule> builder)
    {
        // Time-range overlap conflicts (same room/invigilator/student double-booked)
        // cannot be expressed as a simple unique index, so they are enforced by
        // ExaminationService in the Application layer. These indexes just make
        // those conflict-detection queries fast.
        builder.HasIndex(s => new { s.ExamRoomId, s.ExamDate });
        builder.HasIndex(s => new { s.InvigilatorId, s.ExamDate });
        builder.HasIndex(s => new { s.CourseId, s.AcademicYearId, s.SemesterId });

        builder.HasOne(s => s.Course)
            .WithMany(c => c.ExamSchedules)
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

        builder.HasOne(s => s.ExamRoom)
            .WithMany(r => r.ExamSchedules)
            .HasForeignKey(s => s.ExamRoomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Invigilator)
            .WithMany(i => i.ExamSchedules)
            .HasForeignKey(s => s.InvigilatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
