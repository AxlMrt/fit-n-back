using FitnessApp.Modules.Exercises.Application.Enums;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FitnessApp.Modules.Exercises.Infrastructure.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    private readonly ExercisesDbContext _dbContext;

    public ExerciseRepository(ExercisesDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<Exercise?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Exercise?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .Include(e => e.ExerciseTags).ThenInclude(et => et.Tag)
            .Include(e => e.TargetMuscleGroups).ThenInclude(tmg => tmg.MuscleGroup)
            .Include(e => e.RequiredEquipment).ThenInclude(re => re.Equipment)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Exercise>> GetAllAsync()
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exercise>> GetAllWithDetailsAsync()
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .Include(e => e.ExerciseTags).ThenInclude(et => et.Tag)
            .Include(e => e.TargetMuscleGroups).ThenInclude(tmg => tmg.MuscleGroup)
            .Include(e => e.RequiredEquipment).ThenInclude(re => re.Equipment)
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<Exercise?> GetByNameAsync(string name)
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<Exercise>> GetByTagIdAsync(Guid tagId)
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .Where(e => e.ExerciseTags.Any(et => et.TagId == tagId))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exercise>> GetByMuscleGroupIdAsync(Guid muscleGroupId, bool? isPrimary = null)
    {
        var query = _dbContext.Exercises
            .AsNoTracking()
            .Where(e => e.TargetMuscleGroups.Any(tmg => tmg.MuscleGroupId == muscleGroupId));

        if (isPrimary.HasValue)
        {
            query = query.Where(e => e.TargetMuscleGroups
                .Any(tmg => tmg.MuscleGroupId == muscleGroupId && tmg.IsPrimary == isPrimary.Value));
        }

        return await query.OrderBy(e => e.Name).ToListAsync();
    }

    public async Task<IEnumerable<Exercise>> GetByEquipmentIdAsync(Guid equipmentId)
    {
        return await _dbContext.Exercises
            .AsNoTracking()
            .Where(e => e.RequiredEquipment.Any(re => re.EquipmentId == equipmentId))
            .OrderBy(e => e.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Exercise>> SearchAsync(
        string? name = null,
        IEnumerable<Guid>? tagIds = null,
        IEnumerable<Guid>? muscleGroupIds = null,
        IEnumerable<Guid>? equipmentIds = null,
        DifficultyLevel? difficulty = null,
        int? maxDuration = null,
        bool? requiresEquipment = null,
        int skip = 0,
        int take = 20,
        string? sortBy = "Name",
        bool sortDescending = false)
    {
        var query = _dbContext.Exercises.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(e => e.Name.ToLower().Contains(name.ToLower()));
        }

        if (tagIds != null && tagIds.Any())
        {
            var tagIdsList = tagIds.ToList();
            query = query.Where(e => e.ExerciseTags.Any(et => tagIdsList.Contains(et.TagId)));
        }

        if (muscleGroupIds != null && muscleGroupIds.Any())
        {
            var muscleGroupIdsList = muscleGroupIds.ToList();
            query = query.Where(e => e.TargetMuscleGroups.Any(tmg => muscleGroupIdsList.Contains(tmg.MuscleGroupId)));
        }

        if (equipmentIds != null && equipmentIds.Any())
        {
            var equipmentIdsList = equipmentIds.ToList();
            query = query.Where(e => e.RequiredEquipment.Any(re => equipmentIdsList.Contains(re.EquipmentId)));
        }

        if (difficulty.HasValue)
        {
            query = query.Where(e => e.DifficultyLevel == difficulty.ToString());
        }

        if (requiresEquipment.HasValue)
        {
            query = requiresEquipment.Value
                ? query.Where(e => e.RequiredEquipment.Any())
                : query.Where(e => !e.RequiredEquipment.Any() || e.IsBodyweightExercise);
        }

        // Apply sorting
        query = ApplySorting(query, sortBy, sortDescending);

        // Include related data
        query = query
            .Include(e => e.ExerciseTags).ThenInclude(et => et.Tag)
            .Include(e => e.TargetMuscleGroups).ThenInclude(tmg => tmg.MuscleGroup)
            .Include(e => e.RequiredEquipment).ThenInclude(re => re.Equipment);

        // Apply pagination
        return await query
            .Skip(skip)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsync(Exercise exercise)
    {
        await _dbContext.Exercises.AddAsync(exercise);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Exercise exercise)
    {
        _dbContext.Exercises.Update(exercise);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var exercise = await _dbContext.Exercises.FindAsync(id);
        if (exercise != null)
        {
            _dbContext.Exercises.Remove(exercise);
            await _dbContext.SaveChangesAsync();
        }
    }

    private static IQueryable<Exercise> ApplySorting(IQueryable<Exercise> query, string? sortBy, bool sortDescending)
    {
        Expression<Func<Exercise, object>> keySelector = sortBy?.ToLower() switch
        {
            "name" => e => e.Name,
            "difficulty" => e => e.DifficultyLevel,
            "createdat" => e => e.CreatedAt,
            "updatedat" => e => e.UpdatedAt ?? DateTime.MinValue,
            _ => e => e.Name
        };

        return sortDescending
            ? query.OrderByDescending(keySelector)
            : query.OrderBy(keySelector);
    }
}
