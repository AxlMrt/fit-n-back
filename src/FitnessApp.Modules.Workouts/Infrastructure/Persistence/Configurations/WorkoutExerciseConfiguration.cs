using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Workouts.Domain.Entities;

namespace FitnessApp.Modules.Workouts.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WorkoutExercise entity
/// </summary>
internal class WorkoutExerciseConfiguration : IEntityTypeConfiguration<WorkoutExercise>
{
    public void Configure(EntityTypeBuilder<WorkoutExercise> builder)
    {
        builder.ToTable("workout_exercises");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        // Properties
        builder.Property(e => e.ExerciseId)
            .IsRequired();

        builder.Property(e => e.Sets);

        builder.Property(e => e.Reps);

        builder.Property(e => e.DurationSeconds)
            .HasColumnName("duration_seconds");

        builder.Property(e => e.Distance)
            .HasPrecision(10, 2)
            .HasColumnName("distance_meters");

        builder.Property(e => e.Weight)
            .HasPrecision(5, 2)
            .HasColumnName("weight_kg");

        builder.Property(e => e.RestSeconds)
            .HasColumnName("rest_seconds");

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        builder.Property(e => e.Order)
            .IsRequired();

        // Foreign key
        builder.Property(e => e.WorkoutPhaseId)
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.WorkoutPhase)
            .WithMany(p => p.Exercises)
            .HasForeignKey(e => e.WorkoutPhaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.WorkoutPhaseId)
            .HasDatabaseName("ix_workout_exercises_phase_id");

        builder.HasIndex(e => e.ExerciseId)
            .HasDatabaseName("ix_workout_exercises_exercise_id");

        builder.HasIndex(e => e.Order)
            .HasDatabaseName("ix_workout_exercises_order");

        builder.HasIndex(e => new { e.WorkoutPhaseId, e.Order })
            .IsUnique()
            .HasDatabaseName("ix_workout_exercises_phase_order");

        builder.HasIndex(e => new { e.WorkoutPhaseId, e.ExerciseId })
            .IsUnique()
            .HasDatabaseName("ix_workout_exercises_phase_exercise");
    }
}
