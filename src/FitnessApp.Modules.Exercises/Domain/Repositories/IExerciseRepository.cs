using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Specifications;

namespace FitnessApp.Modules.Exercises.Domain.Repositories
{
    public interface IExerciseRepository
    {
        Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Exercise?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Exercise>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<Exercise>> GetBySpecificationAsync(Specification<Exercise> specification, CancellationToken cancellationToken = default);
        Task<(IEnumerable<Exercise> Items, int TotalCount)> GetPagedAsync(
            int pageNumber, 
            int pageSize, 
            Specification<Exercise>? specification = null,
            string sortBy = "Name", 
            bool sortDescending = false, 
            CancellationToken cancellationToken = default);
        Task<int> CountAsync(Specification<Exercise>? specification = null, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task AddAsync(Exercise entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(Exercise entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(Exercise entity, CancellationToken cancellationToken = default);
    }
}
