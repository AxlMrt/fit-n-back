using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;

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

        // Duration value object conversion
        builder.Property(w => w.EstimatedDuration)
            .IsRequired()
            .HasConversion(
                duration => duration.Value.TotalMinutes,
                minutes => Duration.FromMinutes((int)minutes));

        builder.Property(w => w.RequiredEquipment)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(w => w.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(w => w.ImageContentId);

        builder.Property(w => w.CreatedByUserId);

        builder.Property(w => w.CreatedByCoachId);

        builder.Property(w => w.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(w => w.UpdatedAt);

        // Navigation properties
        builder.HasMany(w => w.Phases)
            .WithOne(p => p.Workout)
            .HasForeignKey("WorkoutId")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(w => w.Name);
        builder.HasIndex(w => w.Type);
        builder.HasIndex(w => w.Difficulty);
        builder.HasIndex(w => w.RequiredEquipment);
        builder.HasIndex(w => w.CreatedByUserId);
        builder.HasIndex(w => w.CreatedByCoachId);
        builder.HasIndex(w => w.CreatedAt);
        builder.HasIndex(w => w.IsActive);

        // Composite indexes
        builder.HasIndex(w => new { w.Type, w.Difficulty });
        builder.HasIndex(w => new { w.IsActive, w.CreatedAt });
    }
}
