using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WorkoutPhase entity
/// </summary>
internal class WorkoutPhaseConfiguration : IEntityTypeConfiguration<WorkoutPhase>
{
    public void Configure(EntityTypeBuilder<WorkoutPhase> builder)
    {
        builder.ToTable("workout_phases");

        // Primary key
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        // Properties
        builder.Property(p => p.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.Property(p => p.EstimatedDurationMinutes)
            .IsRequired()
            .HasColumnName("estimated_duration_minutes");

        builder.Property(p => p.Order)
            .IsRequired();

        // Foreign key
        builder.Property(p => p.WorkoutId)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.Workout)
            .WithMany(w => w.Phases)
            .HasForeignKey(p => p.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Exercises)
            .WithOne(e => e.WorkoutPhase)
            .HasForeignKey("WorkoutPhaseId")
            .OnDelete(DeleteBehavior.Cascade);

        // Computed properties (ignored)
        builder.Ignore(p => p.ExerciseCount);

        // Indexes
        builder.HasIndex(p => p.WorkoutId)
            .HasDatabaseName("ix_workout_phases_workout_id");

        builder.HasIndex(p => p.Type)
            .HasDatabaseName("ix_workout_phases_type");

        builder.HasIndex(p => p.Order)
            .HasDatabaseName("ix_workout_phases_order");

        builder.HasIndex(p => new { p.WorkoutId, p.Order })
            .IsUnique()
            .HasDatabaseName("ix_workout_phases_workout_order");
    }
}
