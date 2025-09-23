using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Tracking.Domain.Entities;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WorkoutSessionSet entity
/// </summary>
internal class WorkoutSessionSetConfiguration : IEntityTypeConfiguration<WorkoutSessionSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSessionSet> builder)
    {
        builder.ToTable("workout_session_sets", "tracking", t =>
        {
            // Check constraints - using PostgreSQL syntax
            t.HasCheckConstraint("ck_workout_session_sets_repetitions_positive", "repetitions IS NULL OR repetitions > 0");
            t.HasCheckConstraint("ck_workout_session_sets_weight_non_negative", "weight_kg IS NULL OR weight_kg >= 0");
            t.HasCheckConstraint("ck_workout_session_sets_duration_positive", "duration_seconds IS NULL OR duration_seconds > 0");
            t.HasCheckConstraint("ck_workout_session_sets_distance_non_negative", "distance_meters IS NULL OR distance_meters >= 0");
            t.HasCheckConstraint("ck_workout_session_sets_rest_time_non_negative", "rest_time_seconds IS NULL OR rest_time_seconds >= 0");
            t.HasCheckConstraint("ck_workout_session_sets_set_number_positive", "set_number > 0");
        });

        // Primary key
        builder.HasKey(wss => wss.Id);
        builder.Property(wss => wss.Id).ValueGeneratedNever();

        // Properties
        builder.Property(wss => wss.WorkoutSessionExerciseId)
            .IsRequired()
            .HasColumnName("workout_session_exercise_id");

        builder.Property(wss => wss.SetNumber)
            .IsRequired()
            .HasColumnName("set_number");

        builder.Property(wss => wss.Repetitions)
            .HasColumnName("repetitions");

        builder.Property(wss => wss.Weight)
            .HasPrecision(6, 2)
            .HasColumnName("weight_kg");

        builder.Property(wss => wss.DurationSeconds)
            .HasColumnName("duration_seconds");

        builder.Property(wss => wss.Distance)
            .HasPrecision(10, 2)
            .HasColumnName("distance_meters");

        builder.Property(wss => wss.CompletedAt)
            .IsRequired()
            .HasColumnName("completed_at");

        builder.Property(wss => wss.RestTimeSeconds)
            .HasColumnName("rest_time_seconds");

        // Relationships
        builder.HasOne(wss => wss.WorkoutSessionExercise)
            .WithMany(wse => wse.Sets)
            .HasForeignKey(wss => wss.WorkoutSessionExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(wss => wss.WorkoutSessionExerciseId)
            .HasDatabaseName("ix_workout_session_sets_exercise_id");

        builder.HasIndex(wss => new { wss.WorkoutSessionExerciseId, wss.SetNumber })
            .IsUnique()
            .HasDatabaseName("ix_workout_session_sets_exercise_set_unique");

        builder.HasIndex(wss => wss.CompletedAt)
            .HasDatabaseName("ix_workout_session_sets_completed_at");
    }
}
