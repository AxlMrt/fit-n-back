using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Application.Interfaces;

/// <summary>
/// Service interface for workout operations providing comprehensive workout management
/// </summary>
public interface IWorkoutService
{
    #region Basic CRUD Operations
    
    Task<WorkoutDto> CreateWorkoutAsync(CreateWorkoutDto createWorkoutDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto?> GetWorkoutByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutDto>> GetWorkoutsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateWorkoutAsync(Guid id, UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteWorkoutAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Query and Filtering Operations
    
    Task<IEnumerable<WorkoutListDto>> GetActiveWorkoutsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutListDto>> GetWorkoutsByTypeAsync(WorkoutType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutListDto>> GetWorkoutsByCategoryAsync(WorkoutCategory category, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutListDto>> GetWorkoutsByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutListDto>> GetWorkoutsByCategoryAndDifficultyAsync(WorkoutCategory category, DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutListDto>> GetTemplateWorkoutsAsync(CancellationToken cancellationToken = default);
    
    // Advanced filtering and search
    Task<IEnumerable<WorkoutListDto>> SearchWorkoutsAsync(string searchTerm, WorkoutCategory? category = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutListDto>> GetWorkoutsWithAdvancedFiltersAsync(
        WorkoutType? type = null,
        WorkoutCategory? category = null,
        DifficultyLevel? difficulty = null,
        int? minDurationMinutes = null,
        int? maxDurationMinutes = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    #endregion

    #region User and Coach Specific Operations
    
    Task<IEnumerable<WorkoutListDto>> GetUserWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutListDto>> GetCoachWorkoutsAsync(Guid coachId, CancellationToken cancellationToken = default);

    #endregion

    #region Pagination and Statistics
    
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<WorkoutListDto> Workouts, int TotalCount)> GetPagedWorkoutsAsync(
        int page, int pageSize, WorkoutType? type = null, WorkoutCategory? category = null, DifficultyLevel? difficulty = null,
        string? searchTerm = null, CancellationToken cancellationToken = default);

    #endregion

    #region Workout Management Operations
    
    Task<WorkoutDto> DuplicateWorkoutAsync(Guid id, string newName, CancellationToken cancellationToken = default);
    Task<bool> DeactivateWorkoutAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ReactivateWorkoutAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Workout Phase Management
    
    Task<WorkoutDto> AddPhaseToWorkoutAsync(Guid workoutId, AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateWorkoutPhaseAsync(Guid workoutId, Guid phaseId, UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> RemovePhaseFromWorkoutAsync(Guid workoutId, Guid phaseId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> MoveWorkoutPhaseAsync(Guid workoutId, Guid phaseId, int newOrder, CancellationToken cancellationToken = default);

    #endregion

    #region Workout Exercise Management
    
    Task<WorkoutDto> AddExerciseToPhaseAsync(Guid workoutId, Guid phaseId, AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdatePhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, UpdateWorkoutExerciseDto updateDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> RemoveExerciseFromPhaseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> MovePhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, int newOrder, CancellationToken cancellationToken = default);

    #endregion
}
