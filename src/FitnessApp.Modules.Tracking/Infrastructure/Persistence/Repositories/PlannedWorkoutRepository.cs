using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.Modules.Tracking.Domain.Repositories;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for PlannedWorkout entity
/// </summary>
internal class PlannedWorkoutRepository : IPlannedWorkoutRepository
{
    private readonly TrackingDbContext _context;

    public PlannedWorkoutRepository(TrackingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PlannedWorkout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedWorkouts
            .FirstOrDefaultAsync(pw => pw.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PlannedWorkout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedWorkouts
            .Where(pw => pw.UserId == userId)
            .OrderBy(pw => pw.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlannedWorkout>> GetUpcomingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PlannedWorkouts
            .Where(pw => pw.UserId == userId &&
                        pw.Status == WorkoutSessionStatus.Planned &&
                        pw.ScheduledDate >= today)
            .OrderBy(pw => pw.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlannedWorkout>> GetOverdueAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        return await _context.PlannedWorkouts
            .Where(pw => pw.UserId == userId &&
                        pw.Status == WorkoutSessionStatus.Planned &&
                        pw.ScheduledDate < today)
            .OrderByDescending(pw => pw.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlannedWorkout>> GetByDateAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedWorkouts
            .Where(pw => pw.UserId == userId && pw.ScheduledDate.Date == date.Date)
            .OrderBy(pw => pw.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlannedWorkout>> GetByStatusAsync(Guid userId, WorkoutSessionStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedWorkouts
            .Where(pw => pw.UserId == userId && pw.Status == status)
            .OrderBy(pw => pw.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PlannedWorkout>> GetByProgramIdAsync(Guid programId, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedWorkouts
            .Where(pw => pw.ProgramId == programId)
            .OrderBy(pw => pw.ScheduledDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasPlannedWorkoutAsync(Guid userId, Guid workoutId, DateTime date, CancellationToken cancellationToken = default)
    {
        return await _context.PlannedWorkouts
            .AnyAsync(pw => pw.UserId == userId &&
                           pw.WorkoutId == workoutId &&
                           pw.ScheduledDate.Date == date.Date &&
                           pw.Status == WorkoutSessionStatus.Planned, cancellationToken);
    }

    public async Task AddAsync(PlannedWorkout plannedWorkout, CancellationToken cancellationToken = default)
    {
        await _context.PlannedWorkouts.AddAsync(plannedWorkout, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(PlannedWorkout plannedWorkout, CancellationToken cancellationToken = default)
    {
        _context.PlannedWorkouts.Update(plannedWorkout);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(PlannedWorkout plannedWorkout, CancellationToken cancellationToken = default)
    {
        _context.PlannedWorkouts.Remove(plannedWorkout);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
