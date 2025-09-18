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
    
    // User operations (UserCreated workouts only)
    Task<WorkoutDto> CreateUserWorkoutAsync(CreateWorkoutDto createWorkoutDto, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateUserWorkoutAsync(Guid id, UpdateWorkoutDto updateDto, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserWorkoutAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    
    // Admin operations (Template workouts and all workouts)
    Task<WorkoutDto> CreateTemplateWorkoutAsync(CreateWorkoutDto createWorkoutDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateWorkoutAsAdminAsync(Guid id, UpdateWorkoutDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteWorkoutAsAdminAsync(Guid id, CancellationToken cancellationToken = default);
    
    // General read operations (accessible to all)
    Task<WorkoutDto?> GetWorkoutByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutDto>> GetWorkoutsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

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
    
    Task<WorkoutDto> DuplicateUserWorkoutAsync(Guid id, string newName, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> DuplicateWorkoutAsAdminAsync(Guid id, string newName, CancellationToken cancellationToken = default);
    Task<bool> DeactivateUserWorkoutAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ReactivateUserWorkoutAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeactivateWorkoutAsAdminAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ReactivateWorkoutAsAdminAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Workout Phase Management
    
    // User operations (own workouts only)
    Task<WorkoutDto> AddPhaseToUserWorkoutAsync(Guid workoutId, AddWorkoutPhaseDto phaseDto, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateUserWorkoutPhaseAsync(Guid workoutId, Guid phaseId, UpdateWorkoutPhaseDto updateDto, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> RemovePhaseFromUserWorkoutAsync(Guid workoutId, Guid phaseId, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> MoveUserWorkoutPhaseAsync(Guid workoutId, Guid phaseId, int newOrder, Guid userId, CancellationToken cancellationToken = default);
    
    // Admin operations (all workouts)
    Task<WorkoutDto> AddPhaseToWorkoutAsAdminAsync(Guid workoutId, AddWorkoutPhaseDto phaseDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateWorkoutPhaseAsAdminAsync(Guid workoutId, Guid phaseId, UpdateWorkoutPhaseDto updateDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> RemovePhaseFromWorkoutAsAdminAsync(Guid workoutId, Guid phaseId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> MoveWorkoutPhaseAsAdminAsync(Guid workoutId, Guid phaseId, int newOrder, CancellationToken cancellationToken = default);

    #endregion

    #region Workout Exercise Management
    
    // User operations (own workouts only)
    Task<WorkoutDto> AddExerciseToUserPhaseAsync(Guid workoutId, Guid phaseId, AddWorkoutExerciseDto exerciseDto, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdateUserPhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, UpdateWorkoutExerciseDto updateDto, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> RemoveExerciseFromUserPhaseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, Guid userId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> MoveUserPhaseExerciseAsync(Guid workoutId, Guid phaseId, Guid exerciseId, int newOrder, Guid userId, CancellationToken cancellationToken = default);
    
    // Admin operations (all workouts)
    Task<WorkoutDto> AddExerciseToPhaseAsAdminAsync(Guid workoutId, Guid phaseId, AddWorkoutExerciseDto exerciseDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> UpdatePhaseExerciseAsAdminAsync(Guid workoutId, Guid phaseId, Guid exerciseId, UpdateWorkoutExerciseDto updateDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> RemoveExerciseFromPhaseAsAdminAsync(Guid workoutId, Guid phaseId, Guid exerciseId, CancellationToken cancellationToken = default);
    Task<WorkoutDto> MovePhaseExerciseAsAdminAsync(Guid workoutId, Guid phaseId, Guid exerciseId, int newOrder, CancellationToken cancellationToken = default);

    #endregion
}
