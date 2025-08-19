using FitnessApp.Modules.Exercises.Domain.Entities;

namespace FitnessApp.Modules.Exercises.Domain.Repositories
{
    public interface IExerciseRepository
    {
        Task<Exercise?> GetByIdAsync(Guid id);
        Task<IEnumerable<Exercise>> GetAllAsync();
        Task AddAsync(Exercise entity);
        Task UpdateAsync(Exercise entity);
        Task DeleteAsync(Exercise entity);
    }
}
