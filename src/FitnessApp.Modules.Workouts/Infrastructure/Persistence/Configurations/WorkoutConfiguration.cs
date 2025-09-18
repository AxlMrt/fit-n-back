using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for Workout entity
/// </summary>
internal class WorkoutConfiguration : IEntityTypeConfiguration<Workout>
{
    public void Configure(EntityTypeBuilder<Workout> builder)
    {
        builder.ToTable("workouts");

        // Primary key
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();

        // Properties
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.Type)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(w => w.Difficulty)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(w => w.EstimatedDurationMinutes)
            .IsRequired()
            .HasColumnName("estimated_duration_minutes");

        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(w => w.ImageContentId);

        // User/Coach references
        builder.Property(w => w.CreatedByUserId);
        builder.Property(w => w.CreatedByCoachId);

        // Audit properties
        builder.Property(w => w.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(w => w.UpdatedAt);

        // Relationships
        builder.HasMany(w => w.Phases)
            .WithOne(p => p.Workout)
            .HasForeignKey("WorkoutId")
            .OnDelete(DeleteBehavior.Cascade);

        // Computed properties (ignored)
        builder.Ignore(w => w.PhaseCount);
        builder.Ignore(w => w.TotalExercises);

        // Indexes
        builder.HasIndex(w => w.Name)
            .HasDatabaseName("ix_workouts_name");

        builder.HasIndex(w => w.Type)
            .HasDatabaseName("ix_workouts_type");

        builder.HasIndex(w => w.Difficulty)
            .HasDatabaseName("ix_workouts_difficulty");

        builder.HasIndex(w => w.CreatedByUserId)
            .HasDatabaseName("ix_workouts_created_by_user");

        builder.HasIndex(w => w.CreatedByCoachId)
            .HasDatabaseName("ix_workouts_created_by_coach");

        builder.HasIndex(w => w.IsActive)
            .HasDatabaseName("ix_workouts_is_active");

        builder.HasIndex(w => w.CreatedAt)
            .HasDatabaseName("ix_workouts_created_at");
    }
}
