using FitnessApp.Modules.Workouts.Application.Interfaces;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Application.Services;

/// <summary>
/// Implementation of workout authorization service
/// </summary>
public class WorkoutAuthorizationService : IWorkoutAuthorizationService
{
    public Task<bool> CanViewWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        if (workout == null)
            return Task.FromResult(false);

        // System workouts (Dynamic, Fixed without specific creator) are viewable by everyone
        if (workout.Type == WorkoutType.Dynamic || 
            (workout.Type == WorkoutType.Fixed && !workout.CreatedByUserId.HasValue && !workout.CreatedByCoachId.HasValue))
        {
            return Task.FromResult(true);
        }

        // User-created workouts are only viewable by their creator
        if (workout.Type == WorkoutType.UserCreated || workout.CreatedByUserId.HasValue)
        {
            return Task.FromResult(currentUserId.HasValue && workout.CreatedByUserId == currentUserId);
        }

        // Coach-created workouts are viewable by everyone (but modifiable only by coach)
        if (workout.Type == WorkoutType.Fixed && workout.CreatedByCoachId.HasValue)
        {
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public Task<bool> CanModifyWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        if (workout == null || !currentUserId.HasValue)
            return Task.FromResult(false);

        // Users can only modify workouts they created
        if (workout.Type == WorkoutType.UserCreated && workout.CreatedByUserId == currentUserId)
        {
            return Task.FromResult(true);
        }

        // Coaches can modify workouts they created
        // Note: In a real application, you would check if the current user is the coach who created it
        // For now, we assume if CreatedByCoachId matches any user ID, it's allowed
        if (workout.Type == WorkoutType.Fixed && workout.CreatedByCoachId == currentUserId)
        {
            return Task.FromResult(true);
        }

        // System dynamic workouts cannot be modified by regular users
        return Task.FromResult(false);
    }

    public Task<bool> CanDeleteWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        if (workout == null || !currentUserId.HasValue)
            return Task.FromResult(false);

        // Users can only delete workouts they created
        if (workout.Type == WorkoutType.UserCreated && workout.CreatedByUserId == currentUserId)
        {
            return Task.FromResult(true);
        }

        // Coaches can delete workouts they created
        if (workout.Type == WorkoutType.Fixed && workout.CreatedByCoachId == currentUserId)
        {
            return Task.FromResult(true);
        }

        // System dynamic workouts cannot be deleted by regular users
        return Task.FromResult(false);
    }

    public Task<bool> CanCreateWorkoutAsync(Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        // Any authenticated user can create workouts
        return Task.FromResult(currentUserId.HasValue);
    }

    public async Task EnsureCanViewWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanViewWorkoutAsync(workout, currentUserId, cancellationToken))
        {
            throw new WorkoutDomainException("You don't have permission to view this workout");
        }
    }

    public async Task EnsureCanModifyWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanModifyWorkoutAsync(workout, currentUserId, cancellationToken))
        {
            throw new WorkoutDomainException("You don't have permission to modify this workout. You can only modify workouts you created.");
        }
    }

    public async Task EnsureCanDeleteWorkoutAsync(Workout workout, Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanDeleteWorkoutAsync(workout, currentUserId, cancellationToken))
        {
            throw new WorkoutDomainException("You don't have permission to delete this workout. You can only delete workouts you created.");
        }
    }

    public async Task EnsureCanCreateWorkoutAsync(Guid? currentUserId, CancellationToken cancellationToken = default)
    {
        if (!await CanCreateWorkoutAsync(currentUserId, cancellationToken))
        {
            throw new WorkoutDomainException("You must be authenticated to create workouts");
        }
    }
}
