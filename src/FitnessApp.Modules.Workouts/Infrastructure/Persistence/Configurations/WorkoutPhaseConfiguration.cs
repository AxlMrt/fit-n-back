using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;

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

        // Duration value object conversion
        builder.Property(p => p.EstimatedDuration)
            .IsRequired()
            .HasConversion(
                duration => duration.Value.TotalMinutes,
                minutes => Duration.FromMinutes((int)minutes));

        builder.Property(p => p.Order)
            .IsRequired();

        // Foreign keys
        builder.Property<Guid>("WorkoutId")
            .IsRequired();

        // Navigation properties
        builder.HasOne(p => p.Workout)
            .WithMany(w => w.Phases)
            .HasForeignKey("WorkoutId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Exercises)
            .WithOne(e => e.WorkoutPhase)
            .HasForeignKey("WorkoutPhaseId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex("WorkoutId");
        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.Order);
        builder.HasIndex("WorkoutId", "Order")
            .IsUnique();
    }
}
