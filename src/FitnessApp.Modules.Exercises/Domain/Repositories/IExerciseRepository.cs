using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.SharedKernel.DTOs.Requests;

namespace FitnessApp.Modules.Exercises.Domain.Repositories;
public interface IExerciseRepository
{
    Task<Exercise?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Exercise?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exercise>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exercise>> SearchByNameAsync(string searchTerm, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Exercise> Items, int TotalCount)> GetPagedAsync(
        ExerciseQueryDto query, 
        CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Exercise entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Exercise entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Exercise entity, CancellationToken cancellationToken = default);
}
