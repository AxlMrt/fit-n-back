using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Infrastructure.Persistence.Configurations;

namespace FitnessApp.Modules.Workouts.Infrastructure.Persistence;

/// <summary>
/// Entity Framework DbContext for Workouts module
/// </summary>
public class WorkoutsDbContext : DbContext
{
    public WorkoutsDbContext(DbContextOptions<WorkoutsDbContext> options) : base(options)
    {
    }

    public DbSet<Workout> Workouts { get; set; } = null!;
    public DbSet<WorkoutPhase> WorkoutPhases { get; set; } = null!;
    public DbSet<WorkoutExercise> WorkoutExercises { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations
        modelBuilder.ApplyConfiguration(new WorkoutConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutPhaseConfiguration());
        modelBuilder.ApplyConfiguration(new WorkoutExerciseConfiguration());

        // Configure schema
        modelBuilder.HasDefaultSchema("workouts");

        base.OnModelCreating(modelBuilder);
    }
}
