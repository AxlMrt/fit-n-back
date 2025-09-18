using AutoMapper;
using FitnessApp.Modules.Workouts.Application.Interfaces;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Repositories;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Application.Services;

/// <summary>
/// Service implementing comprehensive workout operations with proper domain logic and authorization
/// </summary>
public class WorkoutService : IWorkoutService
{
    private readonly IWorkoutRepository _workoutRepository;
    private readonly IMapper _mapper;

    public WorkoutService(IWorkoutRepository workoutRepository, IMapper mapper)
    {
        _workoutRepository = workoutRepository ?? throw new ArgumentNullException(nameof(workoutRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    #region User Operations (UserCreated workouts only)

    public async Task<WorkoutDto> CreateUserWorkoutAsync(CreateWorkoutDto createWorkoutDto, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(createWorkoutDto);
        
        var workout = _mapper.Map<Workout>(createWorkoutDto);
        workout.SetType(WorkoutType.UserCreated);
        workout.SetCreatedBy(userId);
        
        // Add phases if provided
        foreach (var phaseDto in createWorkoutDto.Phases)
        {
            workout.AddPhase(phaseDto.Type, phaseDto.Name, phaseDto.EstimatedDurationMinutes);
            
            var phase = workout.GetPhase(phaseDto.Type);
            if (phase != null)
            {
                foreach (var exerciseDto in phaseDto.Exercises)
                {
                    phase.AddExercise(
                        exerciseDto.ExerciseId,
                        exerciseDto.Sets,
                        exerciseDto.Reps,
                        exerciseDto.DurationSeconds);
                }
            }
        }

        var createdWorkout = await _workoutRepository.AddAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(createdWorkout);
    }

    public async Task<WorkoutDto> UpdateUserWorkoutAsync(Guid id, UpdateWorkoutDto updateDto, Guid userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateDto);

        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {id} not found");

        ValidateUserOwnership(workout, userId);

        workout.UpdateDetails(
            updateDto.Name ?? workout.Name,
            updateDto.Description,
            updateDto.Difficulty,
            updateDto.EstimatedDurationMinutes);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<bool> DeleteUserWorkoutAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        ValidateUserOwnership(workout, userId);

        await _workoutRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<WorkoutDto> DuplicateUserWorkoutAsync(Guid id, string newName, Guid userId, CancellationToken cancellationToken = default)
    {
        var originalWorkout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (originalWorkout == null)
            throw new WorkoutDomainException($"Workout with ID {id} not found");

        // Create a user workout duplicate
        var createDto = CreateDuplicateDto(originalWorkout, newName);
        return await CreateUserWorkoutAsync(createDto, userId, cancellationToken);
    }

    public async Task<bool> DeactivateUserWorkoutAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        ValidateUserOwnership(workout, userId);

        workout.Deactivate();
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return true;
    }

    public async Task<bool> ReactivateUserWorkoutAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        ValidateUserOwnership(workout, userId);

        workout.Activate();
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return true;
    }

    #endregion

    #region Admin Operations (Template workouts and all workouts)

    public async Task<WorkoutDto> CreateTemplateWorkoutAsync(CreateWorkoutDto createWorkoutDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(createWorkoutDto);
        
        var workout = _mapper.Map<Workout>(createWorkoutDto);
        workout.SetType(WorkoutType.Template);
        
        // Add phases if provided
        foreach (var phaseDto in createWorkoutDto.Phases)
        {
            workout.AddPhase(phaseDto.Type, phaseDto.Name, phaseDto.EstimatedDurationMinutes);
            
            var phase = workout.GetPhase(phaseDto.Type);
            if (phase != null)
            {
                foreach (var exerciseDto in phaseDto.Exercises)
                {
                    phase.AddExercise(
                        exerciseDto.ExerciseId,
                        exerciseDto.Sets,
                        exerciseDto.Reps,
                        exerciseDto.DurationSeconds);
                }
            }
        }

        var createdWorkout = await _workoutRepository.AddAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(createdWorkout);
    }

    public async Task<WorkoutDto> UpdateWorkoutAsAdminAsync(Guid id, UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(updateDto);

        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {id} not found");

        workout.UpdateDetails(
            updateDto.Name ?? workout.Name,
            updateDto.Description,
            updateDto.Difficulty,
            updateDto.EstimatedDurationMinutes);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<bool> DeleteWorkoutAsAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _workoutRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return false;

        await _workoutRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    public async Task<WorkoutDto> DuplicateWorkoutAsAdminAsync(Guid id, string newName, CancellationToken cancellationToken = default)
    {
        var originalWorkout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (originalWorkout == null)
            throw new WorkoutDomainException($"Workout with ID {id} not found");

        // Create a template duplicate
        var createDto = CreateDuplicateDto(originalWorkout, newName);
        return await CreateTemplateWorkoutAsync(createDto, cancellationToken);
    }

    public async Task<bool> DeactivateWorkoutAsAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        workout.Deactivate();
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return true;
    }

    public async Task<bool> ReactivateWorkoutAsAdminAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        workout.Activate();
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return true;
    }

    #endregion

    #region General Read Operations (accessible to all)

    public async Task<WorkoutDto?> GetWorkoutByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        return workout != null ? _mapper.Map<WorkoutDto>(workout) : null;
    }

    public async Task<IEnumerable<WorkoutDto>> GetWorkoutsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetByIdsAsync(ids, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetActiveWorkoutsAsync(CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetActiveWorkoutsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetWorkoutsByTypeAsync(WorkoutType type, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetWorkoutsByTypeAsync(type, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetWorkoutsByCategoryAsync(WorkoutCategory category, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetWorkoutsByCategoryAsync(category, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetWorkoutsByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetWorkoutsByDifficultyAsync(difficulty, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetWorkoutsByCategoryAndDifficultyAsync(WorkoutCategory category, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetWorkoutsByCategoryAndDifficultyAsync(category, difficulty, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetTemplateWorkoutsAsync(CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetTemplateWorkoutsAsync(cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> SearchWorkoutsAsync(string searchTerm, WorkoutCategory? category = null, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.SearchWorkoutsAsync(searchTerm, category, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetWorkoutsWithAdvancedFiltersAsync(
        WorkoutType? type = null,
        WorkoutCategory? category = null,
        DifficultyLevel? difficulty = null,
        int? minDurationMinutes = null,
        int? maxDurationMinutes = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetWorkoutsWithFiltersAsync(
            type, category, difficulty, maxDurationMinutes, minDurationMinutes, !includeInactive, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetUserWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetUserCreatedWorkoutsAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<IEnumerable<WorkoutListDto>> GetCoachWorkoutsAsync(Guid coachId, CancellationToken cancellationToken = default)
    {
        var workouts = await _workoutRepository.GetCoachCreatedWorkoutsAsync(coachId, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _workoutRepository.ExistsAsync(id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _workoutRepository.CountAsync(cancellationToken);
    }

    public async Task<(IEnumerable<WorkoutListDto> Workouts, int TotalCount)> GetPagedWorkoutsAsync(
        int page, int pageSize, WorkoutType? type = null, WorkoutCategory? category = null, DifficultyLevel? difficulty = null,
        string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        var (workouts, totalCount) = await _workoutRepository.GetPagedAsync(page, pageSize, type, category, difficulty, searchTerm, cancellationToken);
        var workoutDtos = _mapper.Map<IEnumerable<WorkoutListDto>>(workouts);
        return (workoutDtos, totalCount);
    }

    #endregion

    #region User Phase Management

    public async Task<WorkoutDto> AddPhaseToUserWorkoutAsync(Guid workoutId, AddWorkoutPhaseDto phaseDto, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        ValidateUserOwnership(workout, userId);

        workout.AddPhase(phaseDto.Type, phaseDto.Name, phaseDto.EstimatedDurationMinutes);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> UpdateUserWorkoutPhaseAsync(Guid workoutId, Guid phaseId, UpdateWorkoutPhaseDto updateDto, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        ValidateUserOwnership(workout, userId);

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.UpdateDetails(updateDto.Name ?? phase.Name, updateDto.Description, updateDto.EstimatedDurationMinutes);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> RemovePhaseFromUserWorkoutAsync(Guid workoutId, Guid phaseId, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        ValidateUserOwnership(workout, userId);

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        workout.RemovePhase(phase.Type);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> MoveUserWorkoutPhaseAsync(Guid workoutId, Guid phaseId, int newOrder, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        ValidateUserOwnership(workout, userId);

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        workout.MovePhase(phase.Type, newOrder);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    #endregion

    #region Admin Phase Management

    public async Task<WorkoutDto> AddPhaseToWorkoutAsAdminAsync(Guid workoutId, AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        workout.AddPhase(phaseDto.Type, phaseDto.Name, phaseDto.EstimatedDurationMinutes);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> UpdateWorkoutPhaseAsAdminAsync(Guid workoutId, Guid phaseId, UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.UpdateDetails(updateDto.Name ?? phase.Name, updateDto.Description, updateDto.EstimatedDurationMinutes);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> RemovePhaseFromWorkoutAsAdminAsync(Guid workoutId, Guid phaseId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        workout.RemovePhase(phase.Type);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> MoveWorkoutPhaseAsAdminAsync(Guid workoutId, Guid phaseId, int newOrder, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        workout.MovePhase(phase.Type, newOrder);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    #endregion

    #region User Exercise Management

    public async Task<WorkoutDto> AddExerciseToUserPhaseAsync(Guid workoutId, Guid phaseId, AddWorkoutExerciseDto exerciseDto, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        ValidateUserOwnership(workout, userId);

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.AddExercise(
            exerciseDto.ExerciseId,
            exerciseDto.Sets,
            exerciseDto.Reps,
            exerciseDto.DurationSeconds);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> UpdateUserPhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, UpdateWorkoutExerciseDto updateDto, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        ValidateUserOwnership(workout, userId);

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        var exercise = phase.GetExercise(exerciseId);
        if (exercise == null)
            throw new WorkoutDomainException($"Exercise with ID {exerciseId} not found");

        if (updateDto.Sets.HasValue || updateDto.Reps.HasValue)
        {
            exercise.UpdateParameters(
                updateDto.Sets ?? exercise.Sets ?? 1,
                updateDto.Reps ?? exercise.Reps ?? 1,
                updateDto.DurationSeconds);
        }

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> RemoveExerciseFromUserPhaseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        ValidateUserOwnership(workout, userId);

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.RemoveExercise(exerciseId);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> MoveUserPhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, int newOrder, Guid userId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        ValidateUserOwnership(workout, userId);

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.MoveExercise(exerciseId, newOrder);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    #endregion

    #region Admin Exercise Management

    public async Task<WorkoutDto> AddExerciseToPhaseAsAdminAsync(Guid workoutId, Guid phaseId, AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.AddExercise(
            exerciseDto.ExerciseId,
            exerciseDto.Sets,
            exerciseDto.Reps,
            exerciseDto.DurationSeconds);

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> UpdatePhaseExerciseAsAdminAsync(Guid workoutId, Guid phaseId, Guid exerciseId, UpdateWorkoutExerciseDto updateDto, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        var exercise = phase.GetExercise(exerciseId);
        if (exercise == null)
            throw new WorkoutDomainException($"Exercise with ID {exerciseId} not found");

        if (updateDto.Sets.HasValue || updateDto.Reps.HasValue)
        {
            exercise.UpdateParameters(
                updateDto.Sets ?? exercise.Sets ?? 1,
                updateDto.Reps ?? exercise.Reps ?? 1,
                updateDto.DurationSeconds);
        }

        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> RemoveExerciseFromPhaseAsAdminAsync(Guid workoutId, Guid phaseId, Guid exerciseId, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.RemoveExercise(exerciseId);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> MovePhaseExerciseAsAdminAsync(Guid workoutId, Guid phaseId, Guid exerciseId, int newOrder, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        var phase = workout.Phases.FirstOrDefault(p => p.Id == phaseId);
        if (phase == null)
            throw new WorkoutDomainException($"Phase with ID {phaseId} not found");

        phase.MoveExercise(exerciseId, newOrder);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    #endregion

    #region Utility Methods

    private void ValidateUserOwnership(Workout workout, Guid userId)
    {
        if (workout.Type != WorkoutType.UserCreated)
        {
            throw new WorkoutDomainException("Only user-created workouts can be modified by users");
        }

        if (workout.CreatedByUserId != userId)
        {
            throw new WorkoutDomainException("User can only modify workouts they created");
        }
    }

    private CreateWorkoutDto CreateDuplicateDto(Workout originalWorkout, string newName)
    {
        return new CreateWorkoutDto
        {
            Name = newName,
            Description = originalWorkout.Description,
            Type = originalWorkout.Type,
            Category = originalWorkout.Category,
            Difficulty = originalWorkout.Difficulty,
            EstimatedDurationMinutes = originalWorkout.EstimatedDurationMinutes,
            Phases = originalWorkout.Phases.Select(p => new CreateWorkoutPhaseDto
            {
                Type = p.Type,
                Name = p.Name,
                EstimatedDurationMinutes = p.EstimatedDurationMinutes,
                Exercises = p.Exercises.Select(e => new CreateWorkoutExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    DurationSeconds = e.DurationSeconds,
                    RestTimeSeconds = e.RestSeconds,
                    Notes = e.Notes
                }).ToList()
            }).ToList()
        };
    }

    #endregion
}
