using FitnessApp.Modules.Exercises.Domain.Entities;

namespace FitnessApp.Modules.Exercises.Domain.Repositories;

public interface IMuscleGroupRepository
{
    Task<MuscleGroup?> GetByIdAsync(Guid id);
    Task<IEnumerable<MuscleGroup>> GetAllAsync();
    Task<MuscleGroup?> GetByNameAsync(string name);
    Task<IEnumerable<MuscleGroup>> GetRelatedMuscleGroupsAsync(Guid muscleGroupId);
    Task AddAsync(MuscleGroup muscleGroup);
    Task UpdateAsync(MuscleGroup muscleGroup);
    Task DeleteAsync(Guid id);
    // Task AddRelatedMuscleGroupAsync(Guid primaryId, Guid relatedId);
    // Task RemoveRelatedMuscleGroupAsync(Guid primaryId, Guid relatedId);
    // Task UpdateRelatedMuscleGroupsAsync(Guid primaryId, IEnumerable<Guid> relatedIds);
}