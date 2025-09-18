using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Domain.Repositories;

/// <summary>
/// Repository interface for Workout aggregate
/// </summary>
public interface IWorkoutRepository
{
    // Basic CRUD operations
    Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
    Task<Workout> AddAsync(Workout workout, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // Query operations
    Task<IEnumerable<Workout>> GetActiveWorkoutsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetWorkoutsByTypeAsync(WorkoutType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetWorkoutsByCategoryAsync(WorkoutCategory category, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetWorkoutsByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetWorkoutsByCategoryAndDifficultyAsync(WorkoutCategory category, DifficultyLevel difficulty, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetTemplateWorkoutsAsync(CancellationToken cancellationToken = default);
    // Task<IEnumerable<Workout>> GetWorkoutsByEquipmentAsync(EquipmentType equipment, CancellationToken cancellationToken = default);
    
    // User-specific operations
    Task<IEnumerable<Workout>> GetUserCreatedWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Workout>> GetCoachCreatedWorkoutsAsync(Guid coachId, CancellationToken cancellationToken = default);
    
    // Advanced queries
    Task<IEnumerable<Workout>> GetWorkoutsWithFiltersAsync(
        WorkoutType? type = null,
        WorkoutCategory? category = null,
        DifficultyLevel? difficulty = null,
        int? maxDurationMinutes = null,
        int? minDurationMinutes = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);
    
    // Search and recommendations
    Task<IEnumerable<Workout>> SearchWorkoutsAsync(string searchTerm, WorkoutCategory? category = null, CancellationToken cancellationToken = default);
    
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    
    // Pagination
    Task<(IEnumerable<Workout> Workouts, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        WorkoutType? type = null,
        WorkoutCategory? category = null,
        DifficultyLevel? difficulty = null,
        //EquipmentType? equipment = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);
}
