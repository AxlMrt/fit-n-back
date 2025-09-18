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
/// Service implementing comprehensive workout operations with proper domain logic
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

    #region Basic CRUD Operations

    public async Task<WorkoutDto> CreateWorkoutAsync(CreateWorkoutDto createWorkoutDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(createWorkoutDto);
        
        var workout = _mapper.Map<Workout>(createWorkoutDto);
        
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

    public async Task<WorkoutDto> UpdateWorkoutAsync(Guid id, UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default)
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

    public async Task<bool> DeleteWorkoutAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _workoutRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return false;

        await _workoutRepository.DeleteAsync(id, cancellationToken);
        return true;
    }

    #endregion

    #region Query and Filtering Operations

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

    #endregion

    #region User and Coach Specific Operations

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

    #endregion

    #region Pagination and Statistics

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

    #region Workout Management Operations

    public async Task<WorkoutDto> DuplicateWorkoutAsync(Guid id, string newName, CancellationToken cancellationToken = default)
    {
        var originalWorkout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (originalWorkout == null)
            throw new WorkoutDomainException($"Workout with ID {id} not found");

        // Create a simple duplicate using the mapper
        var createDto = new CreateWorkoutDto
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

        return await CreateWorkoutAsync(createDto, cancellationToken);
    }

    public async Task<bool> DeactivateWorkoutAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        workout.Deactivate();
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return true;
    }

    public async Task<bool> ReactivateWorkoutAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(id, cancellationToken);
        if (workout == null)
            return false;

        workout.Activate();
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return true;
    }

    #endregion

    #region Workout Phase Management

    public async Task<WorkoutDto> AddPhaseToWorkoutAsync(Guid workoutId, AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default)
    {
        var workout = await _workoutRepository.GetByIdAsync(workoutId, cancellationToken);
        if (workout == null)
            throw new WorkoutDomainException($"Workout with ID {workoutId} not found");

        workout.AddPhase(phaseDto.Type, phaseDto.Name, phaseDto.EstimatedDurationMinutes);
        await _workoutRepository.UpdateAsync(workout, cancellationToken);
        return _mapper.Map<WorkoutDto>(workout);
    }

    public async Task<WorkoutDto> UpdateWorkoutPhaseAsync(Guid workoutId, Guid phaseId, UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default)
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

    public async Task<WorkoutDto> RemovePhaseFromWorkoutAsync(Guid workoutId, Guid phaseId, CancellationToken cancellationToken = default)
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

    public async Task<WorkoutDto> MoveWorkoutPhaseAsync(Guid workoutId, Guid phaseId, int newOrder, CancellationToken cancellationToken = default)
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

    #region Workout Exercise Management

    public async Task<WorkoutDto> AddExerciseToPhaseAsync(Guid workoutId, Guid phaseId, AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default)
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

    public async Task<WorkoutDto> UpdatePhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, UpdateWorkoutExerciseDto updateDto, CancellationToken cancellationToken = default)
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

    public async Task<WorkoutDto> RemoveExerciseFromPhaseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, CancellationToken cancellationToken = default)
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

    public async Task<WorkoutDto> MovePhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, int newOrder, CancellationToken cancellationToken = default)
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
}
