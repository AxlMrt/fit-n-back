using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Tracking.Domain.Entities;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for UserMetric entity
/// </summary>
internal class UserMetricConfiguration : IEntityTypeConfiguration<UserMetric>
{
    public void Configure(EntityTypeBuilder<UserMetric> builder)
    {
        builder.ToTable("user_metrics", "tracking");

        // Primary key
        builder.HasKey(um => um.Id);
        builder.Property(um => um.Id).ValueGeneratedNever();

        // Properties
        builder.Property(um => um.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(um => um.MetricType)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("metric_type");

        builder.Property(um => um.Value)
            .IsRequired()
            .HasPrecision(10, 4);

        builder.Property(um => um.Unit)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(um => um.RecordedAt)
            .IsRequired()
            .HasColumnName("recorded_at");

        builder.Property(um => um.Notes)
            .HasMaxLength(500);

        builder.Property(um => um.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(um => um.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(um => um.UserId);
        builder.HasIndex(um => um.MetricType);
        builder.HasIndex(um => new { um.UserId, um.MetricType });
        builder.HasIndex(um => new { um.UserId, um.MetricType, um.RecordedAt });
        builder.HasIndex(um => um.RecordedAt);
    }
}
