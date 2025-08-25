using FitnessApp.Modules.Workouts.Application.DTOs;

namespace FitnessApp.Modules.Workouts.Application.Interfaces;

/// <summary>
/// Service interface for workout operations
/// </summary>
public interface IWorkoutService
{
    // CRUD operations
    Task<WorkoutDto> CreateWorkoutAsync(CreateWorkoutDto createDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto?> GetWorkoutByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateWorkoutAsync(Guid id, UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteWorkoutAsync(Guid id, CancellationToken cancellationToken = default);

    // Query operations
    Task<WorkoutPagedResultDto> GetWorkoutsAsync(WorkoutQueryDto query, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSummaryDto>> GetUserWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutSummaryDto>> GetCoachWorkoutsAsync(Guid coachId, CancellationToken cancellationToken = default);

    // Phase operations
    Task<WorkoutDto> AddPhaseToWorkoutAsync(Guid workoutId, AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateWorkoutPhaseAsync(Guid workoutId, Guid phaseId, UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> RemovePhaseFromWorkoutAsync(Guid workoutId, Guid phaseId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> MoveWorkoutPhaseAsync(Guid workoutId, Guid phaseId, int newOrder, CancellationToken cancellationToken = default);

    // Exercise operations
    Task<WorkoutDto> AddExerciseToPhaseAsync(Guid workoutId, Guid phaseId, AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdatePhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, UpdateWorkoutExerciseDto updateDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> RemoveExerciseFromPhaseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> MovePhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, int newOrder, CancellationToken cancellationToken = default);

    // Utility operations
    Task<WorkoutDto> DuplicateWorkoutAsync(Guid workoutId, string newName, CancellationToken cancellationToken = default);
    Task<bool> DeactivateWorkoutAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ReactivateWorkoutAsync(Guid id, CancellationToken cancellationToken = default);
}
