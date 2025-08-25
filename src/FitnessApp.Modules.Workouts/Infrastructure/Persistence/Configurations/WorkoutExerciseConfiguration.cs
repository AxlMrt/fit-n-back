using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;

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

        builder.Property(e => e.ExerciseName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Order)
            .IsRequired();

        // Configure ExerciseParameters as owned type
        builder.OwnsOne(e => e.Parameters, parameters =>
        {
            parameters.Property(p => p.Reps)
                .HasColumnName("reps");

            parameters.Property(p => p.Sets)
                .HasColumnName("sets");

            parameters.Property(p => p.Duration)
                .HasColumnName("duration_seconds")
                .HasConversion(
                    duration => duration.HasValue ? (long)duration.Value.TotalSeconds : (long?)null,
                    seconds => seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : (TimeSpan?)null);

            parameters.Property(p => p.Weight)
                .HasColumnName("weight")
                .HasPrecision(5, 2); // Up to 999.99 kg

            parameters.Property(p => p.RestTime)
                .HasColumnName("rest_time_seconds")
                .HasConversion(
                    restTime => restTime.HasValue ? (long)restTime.Value.TotalSeconds : (long?)null,
                    seconds => seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value) : (TimeSpan?)null);

            parameters.Property(p => p.Notes)
                .HasColumnName("notes")
                .HasMaxLength(500);
        });

        // Foreign keys
        builder.Property<Guid>("WorkoutPhaseId")
            .IsRequired();

        // Navigation properties
        builder.HasOne(e => e.WorkoutPhase)
            .WithMany(p => p.Exercises)
            .HasForeignKey("WorkoutPhaseId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex("WorkoutPhaseId");
        builder.HasIndex(e => e.ExerciseId);
        builder.HasIndex(e => e.Order);
        builder.HasIndex("WorkoutPhaseId", "Order")
            .IsUnique();
    }
}
