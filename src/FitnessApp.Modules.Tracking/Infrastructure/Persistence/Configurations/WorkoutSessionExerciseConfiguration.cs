using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Tracking.Domain.Entities;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WorkoutSessionExercise entity
/// </summary>
internal class WorkoutSessionExerciseConfiguration : IEntityTypeConfiguration<WorkoutSessionExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutSessionExercise> builder)
    {
        builder.ToTable("workout_session_exercises", "tracking");

        // Primary key
        builder.HasKey(wse => wse.Id);
        builder.Property(wse => wse.Id).ValueGeneratedNever();

        // Properties
        builder.Property(wse => wse.WorkoutSessionId)
            .IsRequired()
            .HasColumnName("workout_session_id");

        builder.Property(wse => wse.ExerciseId)
            .IsRequired()
            .HasColumnName("exercise_id");

        builder.Property(wse => wse.ExerciseName)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("exercise_name");

        builder.Property(wse => wse.MetricType)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("metric_type");

        builder.Property(wse => wse.Order)
            .IsRequired();

        builder.Property(wse => wse.PerformanceScore)
            .HasPrecision(5, 2)
            .HasColumnName("performance_score");

        builder.Property(wse => wse.PerformedAt)
            .IsRequired()
            .HasColumnName("performed_at");

        builder.Property(wse => wse.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(wse => wse.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships
        builder.HasMany(wse => wse.Sets)
            .WithOne(s => s.WorkoutSessionExercise)
            .HasForeignKey(s => s.WorkoutSessionExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wse => wse.WorkoutSessionId);
        builder.HasIndex(wse => wse.ExerciseId);
        builder.HasIndex(wse => new { wse.WorkoutSessionId, wse.Order });
        builder.HasIndex(wse => wse.PerformedAt);
    }
}
