using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Exercises.Infrastructure.Repositories;

public class MuscleGroupRepository : IMuscleGroupRepository
{
    private readonly ExercisesDbContext _dbContext;

    public MuscleGroupRepository(ExercisesDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<MuscleGroup?> GetByIdAsync(Guid id)
    {
        return await _dbContext.MuscleGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(mg => mg.Id == id);
    }

    public async Task<IEnumerable<MuscleGroup>> GetAllAsync()
    {
        return await _dbContext.MuscleGroups
            .AsNoTracking()
            .OrderBy(mg => mg.Name)
            .ToListAsync();
    }

    public async Task<MuscleGroup?> GetByNameAsync(string name)
    {
        return await _dbContext.MuscleGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(mg => mg.Name.ToLower() == name.ToLower());
    }

    public async Task<IEnumerable<MuscleGroup>> GetRelatedMuscleGroupsAsync(Guid muscleGroupId)
    {
        // Find exercises that use this muscle group
        var exerciseIds = await _dbContext.ExerciseMuscleGroups
            .Where(emg => emg.MuscleGroupId == muscleGroupId)
            .Select(emg => emg.ExerciseId)
            .ToListAsync();

        // Then find other muscle groups used by those exercises
        return await _dbContext.ExerciseMuscleGroups
            .Where(emg => exerciseIds.Contains(emg.ExerciseId) && emg.MuscleGroupId != muscleGroupId)
            .Select(emg => emg.MuscleGroup)
            .Distinct()
            .ToListAsync();
    }

    public async Task AddAsync(MuscleGroup muscleGroup)
    {
        await _dbContext.MuscleGroups.AddAsync(muscleGroup);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(MuscleGroup muscleGroup)
    {
        _dbContext.MuscleGroups.Update(muscleGroup);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var muscleGroup = await _dbContext.MuscleGroups.FindAsync(id);
        if (muscleGroup != null)
        {
            _dbContext.MuscleGroups.Remove(muscleGroup);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task AddRelatedMuscleGroupAsync(Guid primaryId, Guid relatedId)
    {
        // Verify that both muscle groups exist
        var primaryExists = await _dbContext.MuscleGroups.AnyAsync(mg => mg.Id == primaryId);
        var relatedExists = await _dbContext.MuscleGroups.AnyAsync(mg => mg.Id == relatedId);
        
        if (!primaryExists || !relatedExists)
        {
            throw new ArgumentException("One or both muscle groups do not exist");
        }
        
        // Create an exercise that connects these muscle groups if they aren't already connected
        var alreadyRelated = await AreRelatedMuscleGroupsAsync(primaryId, relatedId);
        
        if (!alreadyRelated)
        {
            // Find exercises that use the primary muscle group
            var exercisesWithPrimary = await _dbContext.ExerciseMuscleGroups
                .Where(emg => emg.MuscleGroupId == primaryId)
                .Select(emg => emg.ExerciseId)
                .ToListAsync();
                
            // Find exercises that use the related muscle group
            var exercisesWithRelated = await _dbContext.ExerciseMuscleGroups
                .Where(emg => emg.MuscleGroupId == relatedId)
                .Select(emg => emg.ExerciseId)
                .ToListAsync();
                
            // Look for common exercises to add the relationship to
            var commonExercises = exercisesWithPrimary.Intersect(exercisesWithRelated).ToList();
            
            if (!commonExercises.Any())
            {
                // If no common exercises exist, this relationship cannot be established
                // with the current data model - the relationship is defined by common exercises
                throw new InvalidOperationException(
                    "Cannot establish relationship between muscle groups that don't share exercises");
            }
        }
    }

    private async Task<bool> AreRelatedMuscleGroupsAsync(Guid muscleGroupId1, Guid muscleGroupId2)
    {
        // Find exercises that use the first muscle group
        var exercisesWithMg1 = await _dbContext.ExerciseMuscleGroups
            .Where(emg => emg.MuscleGroupId == muscleGroupId1)
            .Select(emg => emg.ExerciseId)
            .ToListAsync();
            
        // Check if any of these exercises also use the second muscle group
        return await _dbContext.ExerciseMuscleGroups
            .AnyAsync(emg => exercisesWithMg1.Contains(emg.ExerciseId) && emg.MuscleGroupId == muscleGroupId2);
    }
}
