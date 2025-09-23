using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence.Configurations;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence;

/// <summary>
/// Database context for the Tracking module
/// </summary>
public class TrackingDbContext : DbContext
{
    public TrackingDbContext(DbContextOptions<TrackingDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<WorkoutSession> WorkoutSessions { get; set; } = null!;
    public DbSet<WorkoutSessionExercise> WorkoutSessionExercises { get; set; } = null!;
    public DbSet<UserMetric> UserMetrics { get; set; } = null!;
    public DbSet<PlannedWorkout> PlannedWorkouts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new WorkoutSessionConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutSessionExerciseConfiguration());
        modelBuilder.ApplyConfiguration(new UserMetricConfiguration());
        modelBuilder.ApplyConfiguration(new PlannedWorkoutConfiguration());

        // Set default schema
        modelBuilder.HasDefaultSchema("tracking");
    }
}
