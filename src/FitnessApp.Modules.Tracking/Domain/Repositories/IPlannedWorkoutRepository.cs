using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Domain.Repositories;

/// <summary>
/// Repository interface for PlannedWorkout entity
/// </summary>
public interface IPlannedWorkoutRepository
{
    Task<PlannedWorkout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlannedWorkout>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlannedWorkout>> GetUpcomingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlannedWorkout>> GetOverdueAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlannedWorkout>> GetByDateAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlannedWorkout>> GetByStatusAsync(Guid userId, WorkoutSessionStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<PlannedWorkout>> GetByProgramIdAsync(Guid programId, CancellationToken cancellationToken = default);
    Task<bool> HasPlannedWorkoutAsync(Guid userId, Guid workoutId, DateTime date, CancellationToken cancellationToken = default);
    Task AddAsync(PlannedWorkout plannedWorkout, CancellationToken cancellationToken = default);
    Task UpdateAsync(PlannedWorkout plannedWorkout, CancellationToken cancellationToken = default);
    Task DeleteAsync(PlannedWorkout plannedWorkout, CancellationToken cancellationToken = default);
}
