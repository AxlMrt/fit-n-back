using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FitnessApp.Modules.Tracking.Domain.Entities;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework configuration for PlannedWorkout entity
/// </summary>
internal class PlannedWorkoutConfiguration : IEntityTypeConfiguration<PlannedWorkout>
{
    public void Configure(EntityTypeBuilder<PlannedWorkout> builder)
    {
        builder.ToTable("planned_workouts", "tracking");

        // Primary key
        builder.HasKey(pw => pw.Id);
        builder.Property(pw => pw.Id).ValueGeneratedNever();

        // Properties
        builder.Property(pw => pw.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(pw => pw.WorkoutId)
            .IsRequired()
            .HasColumnName("workout_id");

        builder.Property(pw => pw.ScheduledDate)
            .IsRequired()
            .HasColumnType("date")
            .HasColumnName("scheduled_date");

        builder.Property(pw => pw.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(pw => pw.IsFromProgram)
            .IsRequired()
            .HasColumnName("is_from_program");

        builder.Property(pw => pw.ProgramId)
            .HasColumnName("program_id");

        builder.Property(pw => pw.WorkoutSessionId)
            .HasColumnName("workout_session_id");

        builder.Property(pw => pw.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(pw => pw.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(pw => pw.UserId);
        builder.HasIndex(pw => pw.WorkoutId);
        builder.HasIndex(pw => pw.ScheduledDate);
        builder.HasIndex(pw => pw.Status);
        builder.HasIndex(pw => new { pw.UserId, pw.ScheduledDate });
        builder.HasIndex(pw => new { pw.UserId, pw.Status });
        builder.HasIndex(pw => pw.ProgramId);
        builder.HasIndex(pw => pw.WorkoutSessionId);
    }
}
