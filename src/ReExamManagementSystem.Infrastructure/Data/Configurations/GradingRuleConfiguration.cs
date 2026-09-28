using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReExamManagementSystem.Domain.Entities;

namespace ReExamManagementSystem.Infrastructure.Data.Configurations;

public class GradingRuleConfiguration : IEntityTypeConfiguration<GradingRule>
{
    public void Configure(EntityTypeBuilder<GradingRule> builder)
    {
        builder.Property(g => g.Grade).IsRequired().HasMaxLength(5);
        builder.Property(g => g.MinMark).HasColumnType("decimal(5,2)");
        builder.Property(g => g.MaxMark).HasColumnType("decimal(5,2)");
        builder.Property(g => g.GradePoint).HasColumnType("decimal(4,2)");

        builder.HasIndex(g => g.Grade).IsUnique();
    }
}
