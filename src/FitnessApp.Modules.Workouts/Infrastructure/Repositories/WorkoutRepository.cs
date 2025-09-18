using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.Modules.Workouts.Domain.Repositories;
using FitnessApp.Modules.Workouts.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Infrastructure.Repositories;

/// <summary>
/// Entity Framework implementation of IWorkoutRepository
/// </summary>
public class WorkoutRepository : IWorkoutRepository
{
    private readonly WorkoutsDbContext _context;

    public WorkoutRepository(WorkoutsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Workout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Include(w => w.Phases
                .OrderBy(p => p.Order))
            .ThenInclude(p => p.Exercises
                .OrderBy(e => e.Order))
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Include(w => w.Phases
                .OrderBy(p => p.Order))
            .ThenInclude(p => p.Exercises
                .OrderBy(e => e.Order))
            .Where(w => ids.Contains(w.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<Workout> AddAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(cancellationToken);
        return workout;
    }

    public async Task UpdateAsync(Workout workout, CancellationToken cancellationToken = default)
    {
        _context.Workouts.Update(workout);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var workout = await _context.Workouts.FindAsync([id], cancellationToken);
        if (workout != null)
        {
            _context.Workouts.Remove(workout);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<IEnumerable<Workout>> GetActiveWorkoutsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetWorkoutsByTypeAsync(WorkoutType type, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.Type == type && w.IsActive)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetWorkoutsByDifficultyAsync(DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.Difficulty == difficulty && w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetWorkoutsByCategoryAsync(WorkoutCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.Category == category && w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetWorkoutsByCategoryAndDifficultyAsync(WorkoutCategory category, DifficultyLevel difficulty, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.Category == category && w.Difficulty == difficulty && w.IsActive)
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetTemplateWorkoutsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.Type == WorkoutType.Template && w.IsActive)
            .OrderBy(w => w.Category)
            .ThenBy(w => w.Difficulty)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> SearchWorkoutsAsync(string searchTerm, WorkoutCategory? category = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Workouts.Where(w => w.IsActive);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(w => w.Name.ToLower().Contains(searchTerm) || 
                                   (!string.IsNullOrEmpty(w.Description) && w.Description.ToLower().Contains(searchTerm)));
        }

        if (category.HasValue)
        {
            query = query.Where(w => w.Category == category.Value);
        }

        return await query
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetWorkoutsWithFiltersAsync(
        WorkoutType? type = null,
        WorkoutCategory? category = null,
        DifficultyLevel? difficulty = null,
        int? minDuration = null,
        int? maxDuration = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Workouts.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(w => w.IsActive);
        }

        if (type.HasValue)
        {
            query = query.Where(w => w.Type == type.Value);
        }

        if (category.HasValue)
        {
            query = query.Where(w => w.Category == category.Value);
        }

        if (difficulty.HasValue)
        {
            query = query.Where(w => w.Difficulty == difficulty.Value);
        }

        if (minDuration.HasValue)
        {
            query = query.Where(w => w.EstimatedDurationMinutes >= minDuration.Value);
        }

        if (maxDuration.HasValue)
        {
            query = query.Where(w => w.EstimatedDurationMinutes <= maxDuration.Value);
        }

        return await query
            .OrderBy(w => w.Category)
            .ThenBy(w => w.Difficulty)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Workout> Workouts, int TotalCount)> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        WorkoutType? type = null,
        WorkoutCategory? category = null,
        DifficultyLevel? difficulty = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Workouts.Where(w => w.IsActive);

        if (type.HasValue)
        {
            query = query.Where(w => w.Type == type.Value);
        }

        if (category.HasValue)
        {
            query = query.Where(w => w.Category == category.Value);
        }

        if (difficulty.HasValue)
        {
            query = query.Where(w => w.Difficulty == difficulty.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(w => w.Name.ToLower().Contains(searchTerm) || 
                                   (!string.IsNullOrEmpty(w.Description) && w.Description.ToLower().Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(w => w.Category)
            .ThenBy(w => w.Difficulty)
            .ThenBy(w => w.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    // public async Task<IEnumerable<Workout>> GetWorkoutsByEquipmentAsync(EquipmentType equipment, CancellationToken cancellationToken = default)
    // {
    //     return await _context.Workouts
    //         .Where(w => (w.RequiredEquipment & equipment) == equipment && w.IsActive)
    //         .OrderBy(w => w.Name)
    //         .ToListAsync(cancellationToken);
    // }

    public async Task<IEnumerable<Workout>> GetUserCreatedWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.CreatedByUserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetCoachCreatedWorkoutsAsync(Guid coachId, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts
            .Where(w => w.CreatedByCoachId == coachId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Workout>> GetWorkoutsWithFiltersAsync(
        WorkoutType? type = null,
        DifficultyLevel? difficulty = null,
        //EquipmentType? equipment = null,
        int? maxDurationMinutes = null,
        int? minDurationMinutes = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Workouts.AsQueryable();

        if (activeOnly)
            query = query.Where(w => w.IsActive);

        if (type.HasValue)
            query = query.Where(w => w.Type == type.Value);

        if (difficulty.HasValue)
            query = query.Where(w => w.Difficulty == difficulty.Value);

        // if (equipment.HasValue)
        //     query = query.Where(w => (w.RequiredEquipment & equipment.Value) == equipment.Value);

        if (maxDurationMinutes.HasValue)
            query = query.Where(w => w.EstimatedDurationMinutes <= maxDurationMinutes.Value);

        if (minDurationMinutes.HasValue)
            query = query.Where(w => w.EstimatedDurationMinutes >= minDurationMinutes.Value);

        return await query
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workouts.AnyAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Workouts.CountAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Workout> Workouts, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        WorkoutType? type = null,
        DifficultyLevel? difficulty = null,
        // EquipmentType? equipment = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Workouts.AsQueryable();

        // Apply filters
        query = query.Where(w => w.IsActive);

        if (type.HasValue)
            query = query.Where(w => w.Type == type.Value);

        if (difficulty.HasValue)
            query = query.Where(w => w.Difficulty == difficulty.Value);

        // if (equipment.HasValue)
        //     query = query.Where(w => (w.RequiredEquipment & equipment.Value) == equipment.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(w => 
                w.Name.ToLower().Contains(searchLower) ||
                (w.Description != null && w.Description.ToLower().Contains(searchLower)));
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var workouts = await query
            .OrderBy(w => w.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (workouts, totalCount);
    }
}
