using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Tracking.Domain.Entities;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for WorkoutSession entity
/// </summary>
internal class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
    public void Configure(EntityTypeBuilder<WorkoutSession> builder)
    {
        builder.ToTable("workout_sessions", "tracking");

        // Primary key
        builder.HasKey(ws => ws.Id);
        builder.Property(ws => ws.Id).ValueGeneratedNever();

        // Properties
        builder.Property(ws => ws.UserId)
            .IsRequired();

        builder.Property(ws => ws.WorkoutId)
            .IsRequired();

        builder.Property(ws => ws.PlannedDate)
            .HasColumnName("planned_date");

        builder.Property(ws => ws.StartTime)
            .HasColumnName("start_time");

        builder.Property(ws => ws.EndTime)
            .HasColumnName("end_time");

        builder.Property(ws => ws.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ws => ws.TotalDurationSeconds)
            .HasColumnName("total_duration_seconds");

        builder.Property(ws => ws.CaloriesEstimated)
            .HasColumnName("calories_estimated");

        builder.Property(ws => ws.PerceivedDifficulty)
            .HasColumnName("perceived_difficulty")
            .HasConversion<int?>();

        builder.Property(ws => ws.Notes)
            .HasMaxLength(1000);

        builder.Property(ws => ws.IsFromProgram)
            .IsRequired()
            .HasColumnName("is_from_program");

        builder.Property(ws => ws.ProgramId)
            .HasColumnName("program_id");

        builder.Property(ws => ws.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(ws => ws.UpdatedAt)
            .HasColumnName("updated_at");

        // Relationships - Use the backing field to configure the collection
        builder.HasMany(ws => ws.Exercises)
            .WithOne(wse => wse.WorkoutSession)
            .HasForeignKey(wse => wse.WorkoutSessionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.Metadata.FindNavigation(nameof(WorkoutSession.Exercises))
            ?.SetPropertyAccessMode(PropertyAccessMode.Field);

        // Indexes
        builder.HasIndex(ws => ws.UserId);
        builder.HasIndex(ws => ws.WorkoutId);
        builder.HasIndex(ws => ws.Status);
        builder.HasIndex(ws => new { ws.UserId, ws.Status });
        builder.HasIndex(ws => new { ws.UserId, ws.StartTime });
        builder.HasIndex(ws => ws.PlannedDate);
    }
}
