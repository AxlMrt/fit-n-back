using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.Modules.Tracking.Domain.Repositories;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for WorkoutSession entity
/// </summary>
internal class WorkoutSessionRepository : IWorkoutSessionRepository
{
    private readonly TrackingDbContext _context;

    public WorkoutSessionRepository(TrackingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Include(ws => ws.Exercises.OrderBy(e => e.Order))
            .FirstOrDefaultAsync(ws => ws.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<WorkoutSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Where(ws => ws.UserId == userId)
            .Include(ws => ws.Exercises.OrderBy(e => e.Order))
            .OrderByDescending(ws => ws.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkoutSession>> GetUserWorkoutHistoryAsync(Guid userId, int take = 50, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Where(ws => ws.UserId == userId && ws.Status == WorkoutSessionStatus.Completed)
            .OrderByDescending(ws => ws.EndTime ?? ws.CreatedAt)
            .Take(take)
            .Include(ws => ws.Exercises.OrderBy(e => e.Order))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkoutSession>> GetInProgressSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Where(ws => ws.UserId == userId && ws.Status == WorkoutSessionStatus.InProgress)
            .Include(ws => ws.Exercises.OrderBy(e => e.Order))
            .OrderByDescending(ws => ws.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkoutSession>> GetSessionsInPeriodAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Where(ws => ws.UserId == userId &&
                        ((ws.StartTime.HasValue && ws.StartTime.Value.Date >= startDate.Date && ws.StartTime.Value.Date <= endDate.Date) ||
                         (ws.PlannedDate.HasValue && ws.PlannedDate.Value.Date >= startDate.Date && ws.PlannedDate.Value.Date <= endDate.Date)))
            .Include(ws => ws.Exercises.OrderBy(e => e.Order))
            .OrderBy(ws => ws.StartTime ?? ws.PlannedDate ?? ws.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkoutSession>> GetSessionsByStatusAsync(Guid userId, WorkoutSessionStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Where(ws => ws.UserId == userId && ws.Status == status)
            .Include(ws => ws.Exercises.OrderBy(e => e.Order))
            .OrderByDescending(ws => ws.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<WorkoutSession>> GetSessionsByWorkoutIdAsync(Guid workoutId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Where(ws => ws.WorkoutId == workoutId)
            .Include(ws => ws.Exercises.OrderBy(e => e.Order))
            .OrderByDescending(ws => ws.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .AnyAsync(ws => ws.UserId == userId && ws.Status == WorkoutSessionStatus.InProgress, cancellationToken);
    }

    public async Task<WorkoutSession?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.WorkoutSessions
            .Include(ws => ws.Exercises.OrderBy(e => e.Order))
            .FirstOrDefaultAsync(ws => ws.UserId == userId && ws.Status == WorkoutSessionStatus.InProgress, cancellationToken);
    }

    public async Task AddAsync(WorkoutSession session, CancellationToken cancellationToken = default)
    {
        await _context.WorkoutSessions.AddAsync(session, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(WorkoutSession session, CancellationToken cancellationToken = default)
    {
        _context.WorkoutSessions.Update(session);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(WorkoutSession session, CancellationToken cancellationToken = default)
    {
        _context.WorkoutSessions.Remove(session);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
