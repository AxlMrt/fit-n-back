using FitnessApp.Modules.Workouts.Domain.Entities;

namespace FitnessApp.Modules.Workouts.Application.Interfaces;

/// <summary>
/// Service for workout authorization and access control
/// </summary>
public interface IWorkoutAuthorizationService
{
    /// <summary>
    /// Check if the current user can view the workout
    /// </summary>
    Task<bool> CanViewWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the current user can modify the workout
    /// </summary>
    Task<bool> CanModifyWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the current user can delete the workout
    /// </summary>
    Task<bool> CanDeleteWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if the current user can create workouts
    /// </summary>
    Task<bool> CanCreateWorkoutAsync(Guid? currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensure the current user can view the workout, throw exception if not
    /// </summary>
    Task EnsureCanViewWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensure the current user can modify the workout, throw exception if not
    /// </summary>
    Task EnsureCanModifyWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensure the current user can delete the workout, throw exception if not
    /// </summary>
    Task EnsureCanDeleteWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensure the current user can create workouts, throw exception if not
    /// </summary>
    Task EnsureCanCreateWorkoutAsync(Guid? currentUserId, CancellationToken cancellationToken = default);
}
