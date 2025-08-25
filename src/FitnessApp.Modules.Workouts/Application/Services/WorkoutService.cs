using FitnessApp.Modules.Workouts.Application.DTOs;
using FitnessApp.Modules.Workouts.Application.Interfaces;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.Repositories;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Application.Services;

/// <summary>
/// Service implementation for workout operations
/// </summary>
public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IWorkoutAuthorizationService _authorizationService;
    private readonly ICurrentUserService _currentUserService;

    public WorkoutService(
        IWorkoutRepository workoutRepository, 
        IWorkoutAuthorizationService authorizationService,
        ICurrentUserService currentUserService)
    {
        _workoutRepository = workoutRepository ?? throw new ArgumentNullException(nameof(workoutRepository));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<WorkoutDto> CreateWorkoutAsync(CreateWorkoutDto createDto, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanCreateWorkoutAsync(currentUserId, cancellationToken);

        var duration = Duration.FromMinutes(createDto.EstimatedDurationMinutes);
        
        // Ensure the user can only create workouts for themselves
        var createdByUserId = currentUserId;
        var createdByCoachId = createDto.CreatedByCoachId;

        // If user tries to create for another user, deny
        if (createDto.CreatedByUserId.HasValue && createDto.CreatedByUserId != currentUserId)
        {
            throw new WorkoutDomainException("You can only create workouts for yourself");
        }

        var workout = new Workout(
            createDto.Name,
            createDto.Type,
            createDto.Difficulty,
            duration,
            createDto.RequiredEquipment,
            createdByUserId,
            createdByCoachId);

        if (!string.IsNullOrWhiteSpace(createDto.Description))
        {
            workout.SetDescription(createDto.Description);
        }

        // Add phases if provided
        if (createDto.Phases != null)
        {
            foreach (var phaseDto in createDto.Phases)
            {
                var phaseDuration = Duration.FromMinutes(phaseDto.EstimatedDurationMinutes);
                var phase = workout.AddPhase(phaseDto.Type, phaseDto.Name, phaseDuration);
                
                if (!string.IsNullOrWhiteSpace(phaseDto.Description))
                {
                    phase.SetDescription(phaseDto.Description);
                }

                // Add exercises if provided
                if (phaseDto.Exercises != null)
                {
                    foreach (var exerciseDto in phaseDto.Exercises)
                    {
                        var parameters = CreateExerciseParameters(exerciseDto);
                        phase.AddExercise(exerciseDto.ExerciseId, exerciseDto.ExerciseName, parameters);
                    }
                }
            }
        }

        await _workoutRepository.AddAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto?> GetWorkoutByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return null;

        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanViewWorkoutAsync(workout, currentUserId, cancellationToken);

        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> UpdateWorkoutAsync(Guid id, UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {id} not found");

        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanModifyWorkoutAsync(workout, currentUserId, cancellationToken);

        if (!string.IsNullOrEmpty(updateDto.Name))
            workout.UpdateName(updateDto.Name);

        if (updateDto.Description is not null)
            workout.SetDescription(updateDto.Description);

        if (updateDto.Difficulty.HasValue)
            workout.UpdateDifficulty(updateDto.Difficulty.Value);

        if (updateDto.RequiredEquipment.HasValue)
            workout.UpdateRequiredEquipment(updateDto.RequiredEquipment.Value);

        if (updateDto.ImageContentId.HasValue)
            workout.SetImageContent(updateDto.ImageContentId);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<bool> DeleteWorkoutAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanDeleteWorkoutAsync(workout, currentUserId, cancellationToken);

        await _workoutRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<WorkoutPagedResultDto> GetWorkoutsAsync(WorkoutQueryDto query, CancellationToken cancellationToken = default)
    {
        var (workouts, totalCount) = await _workoutRepository.GetPagedAsync(
            query.Page,
            query.PageSize,
            query.Type,
            query.Difficulty,
            query.Equipment,
            query.SearchTerm,
            cancellationToken);

        var workoutSummaries = workouts.Select(MapToWorkoutSummaryDto).ToList();
        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        return new WorkoutPagedResultDto(workoutSummaries, totalCount, query.Page, query.PageSize, totalPages);
    }

    public async Task<IEnumerable<WorkoutSummaryDto>> GetUserWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetUserCreatedWorkoutsAsync(userId, cancellationToken);
        return workouts.Select(MapToWorkoutSummaryDto);
    }

    public async Task<IEnumerable<WorkoutSummaryDto>> GetCoachWorkoutsAsync(Guid coachId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetCoachCreatedWorkoutsAsync(coachId, cancellationToken);
        return workouts.Select(MapToWorkoutSummaryDto);
    }

    public async Task<WorkoutDto> AddPhaseToWorkoutAsync(Guid workoutId, AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanModifyWorkoutAsync(workout, currentUserId, cancellationToken);

        var duration = Duration.FromMinutes(phaseDto.EstimatedDurationMinutes);
        var phase = workout.AddPhase(phaseDto.Type, phaseDto.Name, duration);
        
        if (!string.IsNullOrWhiteSpace(phaseDto.Description))
        {
            phase.SetDescription(phaseDto.Description);
        }

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> UpdateWorkoutPhaseAsync(Guid workoutId, Guid phaseId, UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanModifyWorkoutAsync(workout, currentUserId, cancellationToken);

        var phase = workout.GetPhaseById(phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        if (updateDto.EstimatedDurationMinutes.HasValue)
        {
            var duration = Duration.FromMinutes(updateDto.EstimatedDurationMinutes.Value);
            phase.UpdateEstimatedDuration(duration);
        }

        if (updateDto.Description is not null)
            phase.SetDescription(updateDto.Description);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> RemovePhaseFromWorkoutAsync(Guid workoutId, Guid phaseId, CancellationToken cancellationToken = default)
    {
        var workout = await GetWorkoutAndEnsureCanModifyAsync(workoutId, cancellationToken);

        workout.RemovePhase(phaseId);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> MoveWorkoutPhaseAsync(Guid workoutId, Guid phaseId, int newOrder, CancellationToken cancellationToken = default)
    {
        var workout = await GetWorkoutAndEnsureCanModifyAsync(workoutId, cancellationToken);

        workout.MovePhase(phaseId, newOrder);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> AddExerciseToPhaseAsync(Guid workoutId, Guid phaseId, AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default)
    {
        var workout = await GetWorkoutAndEnsureCanModifyAsync(workoutId, cancellationToken);

        var phase = workout.GetPhaseById(phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        var parameters = CreateExerciseParameters(exerciseDto);
        phase.AddExercise(exerciseDto.ExerciseId, exerciseDto.ExerciseName, parameters);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> UpdatePhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, UpdateWorkoutExerciseDto updateDto, CancellationToken cancellationToken = default)
    {
        var workout = await GetWorkoutAndEnsureCanModifyAsync(workoutId, cancellationToken);

        var phase = workout.GetPhaseById(phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        var exercise = phase.Exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            throw new WorkoutDomainException($"Exercise with ID {exerciseId} not found in phase");

        var parameters = CreateExerciseParameters(updateDto, exercise.Parameters);
        exercise.UpdateParameters(parameters);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> RemoveExerciseFromPhaseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, CancellationToken cancellationToken = default)
    {
        var workout = await GetWorkoutAndEnsureCanModifyAsync(workoutId, cancellationToken);

        var phase = workout.GetPhaseById(phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.RemoveExercise(exerciseId);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> MovePhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, int newOrder, CancellationToken cancellationToken = default)
    {
        var workout = await GetWorkoutAndEnsureCanModifyAsync(workoutId, cancellationToken);

        var phase = workout.GetPhaseById(phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.MoveExercise(exerciseId, newOrder);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return MapToWorkoutDto(workout);
    }

    public async Task<WorkoutDto> DuplicateWorkoutAsync(Guid workoutId, string newName, CancellationToken cancellationToken = default)
    {
        var originalWorkout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (originalWorkout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var currentUserId = _currentUserService.GetCurrentUserId();
        
        // User must be able to view the original workout to duplicate it
        await _authorizationService.EnsureCanViewWorkoutAsync(originalWorkout, currentUserId, cancellationToken);
        
        // User must be able to create workouts
        await _authorizationService.EnsureCanCreateWorkoutAsync(currentUserId, cancellationToken);

        // Create the duplicated workout with current user as owner
        var duplicatedWorkout = new Workout(
            newName,
            WorkoutType.UserCreated, // Always create as user-created when duplicating
            originalWorkout.Difficulty,
            originalWorkout.EstimatedDuration,
            originalWorkout.RequiredEquipment,
            currentUserId, // Set current user as creator
            null); // No coach ID for user-created workouts

        if (!string.IsNullOrWhiteSpace(originalWorkout.Description))
        {
            duplicatedWorkout.SetDescription(originalWorkout.Description);
        }

        // Duplicate phases and exercises
        foreach (var originalPhase in originalWorkout.Phases.OrderBy(p => p.Order))
        {
            var duplicatedPhase = duplicatedWorkout.AddPhase(
                originalPhase.Type,
                originalPhase.Name,
                originalPhase.EstimatedDuration);

            if (!string.IsNullOrWhiteSpace(originalPhase.Description))
            {
                duplicatedPhase.SetDescription(originalPhase.Description);
            }

            foreach (var originalExercise in originalPhase.Exercises.OrderBy(e => e.Order))
            {
                duplicatedPhase.AddExercise(
                    originalExercise.ExerciseId,
                    originalExercise.ExerciseName,
                    originalExercise.Parameters);
            }
        }

        await _workoutRepository.AddAsync(duplicatedWorkout, cancellationToken);
        return MapToWorkoutDto(duplicatedWorkout);
    }

    public async Task<bool> DeactivateWorkoutAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanModifyWorkoutAsync(workout, currentUserId, cancellationToken);

        workout.Deactivate();
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return true;
    }

    public async Task<bool> ReactivateWorkoutAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanModifyWorkoutAsync(workout, currentUserId, cancellationToken);

        workout.Reactivate();
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return true;
    }

    #region Private Helper Methods

    /// <summary>
    /// Helper method to get workout and ensure current user can modify it
    /// </summary>
    private async Task<Workout> GetWorkoutAndEnsureCanModifyAsync(Guid workoutId, CancellationToken cancellationToken)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var currentUserId = _currentUserService.GetCurrentUserId();
        await _authorizationService.EnsureCanModifyWorkoutAsync(workout, currentUserId, cancellationToken);

        return workout;
    }

    private static ExerciseParameters CreateExerciseParameters(CreateWorkoutExerciseDto dto)
    {
        return new ExerciseParameters(
            dto.Reps,
            dto.Sets,
            dto.DurationSeconds.HasValue ? TimeSpan.FromSeconds(dto.DurationSeconds.Value) : null,
            dto.Weight,
            dto.RestTimeSeconds.HasValue ? TimeSpan.FromSeconds(dto.RestTimeSeconds.Value) : null,
            dto.Notes);
    }

    private static ExerciseParameters CreateExerciseParameters(AddWorkoutExerciseDto dto)
    {
        return new ExerciseParameters(
            dto.Reps,
            dto.Sets,
            dto.DurationSeconds.HasValue ? TimeSpan.FromSeconds(dto.DurationSeconds.Value) : null,
            dto.Weight,
            dto.RestTimeSeconds.HasValue ? TimeSpan.FromSeconds(dto.RestTimeSeconds.Value) : null,
            dto.Notes);
    }

    private static ExerciseParameters CreateExerciseParameters(UpdateWorkoutExerciseDto dto, ExerciseParameters existing)
    {
        return new ExerciseParameters(
            dto.Reps ?? existing.Reps,
            dto.Sets ?? existing.Sets,
            dto.DurationSeconds.HasValue ? TimeSpan.FromSeconds(dto.DurationSeconds.Value) : existing.Duration,
            dto.Weight ?? existing.Weight,
            dto.RestTimeSeconds.HasValue ? TimeSpan.FromSeconds(dto.RestTimeSeconds.Value) : existing.RestTime,
            dto.Notes ?? existing.Notes);
    }

    private static WorkoutDto MapToWorkoutDto(Workout workout)
    {
        return new WorkoutDto(
            workout.Id,
            workout.Name,
            workout.Description,
            workout.Type,
            workout.Difficulty,
            workout.EstimatedDuration.TotalMinutes,
            workout.RequiredEquipment,
            workout.IsActive,
            workout.ImageContentId,
            workout.CreatedByUserId,
            workout.CreatedByCoachId,
            workout.CreatedAt,
            workout.UpdatedAt,
            workout.Phases.OrderBy(p => p.Order).Select(MapToWorkoutPhaseDto).ToList());
    }

    private static WorkoutPhaseDto MapToWorkoutPhaseDto(WorkoutPhase phase)
    {
        return new WorkoutPhaseDto(
            phase.Id,
            phase.Type,
            phase.Name,
            phase.Description,
            phase.EstimatedDuration.TotalMinutes,
            phase.Order,
            phase.Exercises.OrderBy(e => e.Order).Select(MapToWorkoutExerciseDto).ToList());
    }

    private static WorkoutExerciseDto MapToWorkoutExerciseDto(WorkoutExercise exercise)
    {
        return new WorkoutExerciseDto(
            exercise.Id,
            exercise.ExerciseId,
            exercise.ExerciseName,
            exercise.Parameters.Reps,
            exercise.Parameters.Sets,
            exercise.Parameters.Duration?.TotalSeconds > 0 ? (int)exercise.Parameters.Duration.Value.TotalSeconds : null,
            exercise.Parameters.Weight,
            exercise.Parameters.RestTime?.TotalSeconds > 0 ? (int)exercise.Parameters.RestTime.Value.TotalSeconds : null,
            exercise.Parameters.Notes,
            exercise.Order);
    }

    private static WorkoutSummaryDto MapToWorkoutSummaryDto(Workout workout)
    {
        return new WorkoutSummaryDto(
            workout.Id,
            workout.Name,
            workout.Description,
            workout.Type,
            workout.Difficulty,
            workout.EstimatedDuration.TotalMinutes,
            workout.RequiredEquipment,
            workout.IsActive,
            workout.ImageContentId,
            workout.Phases.Count,
            workout.GetTotalExerciseCount(),
            workout.CreatedAt);
    }

    #endregion
}
