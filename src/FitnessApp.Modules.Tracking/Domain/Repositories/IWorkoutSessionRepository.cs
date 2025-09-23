using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Domain.Repositories;

/// <summary>
/// Repository interface for WorkoutSession aggregate
/// </summary>
public interface IWorkoutSessionRepository
{
    Task<WorkoutSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSession>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSession>> GetUserWorkoutHistoryAsync(Guid userId, int take = 50, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSession>> GetInProgressSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSession>> GetSessionsInPeriodAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSession>> GetSessionsByStatusAsync(Guid userId, WorkoutSessionStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSession>> GetSessionsByWorkoutIdAsync(Guid workoutId, CancellationToken cancellationToken = default);
    Task<bool> HasActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutSession?> GetActiveSessionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(WorkoutSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkoutSession session, CancellationToken cancellationToken = default);
    Task DeleteAsync(WorkoutSession session, CancellationToken cancellationToken = default);
}
