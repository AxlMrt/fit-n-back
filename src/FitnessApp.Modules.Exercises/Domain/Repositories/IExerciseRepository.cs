using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Application.Enums;

namespace FitnessApp.Modules.Exercises.Domain.Repositories;

public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(Guid id);
    Task<Exercise?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Exercise>> GetAllAsync();
    Task<IEnumerable<Exercise>> GetAllWithDetailsAsync();
    Task<Exercise?> GetByNameAsync(string name);
    Task<IEnumerable<Exercise>> SearchAsync(
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
        bool sortDescending = false);
    Task AddAsync(Exercise exercise);
    Task UpdateAsync(Exercise exercise);
    Task DeleteAsync(Guid id);
}