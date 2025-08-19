
using FitnessApp.Modules.Exercises.Domain.Entities;

namespace FitnessApp.Modules.Exercises.Domain.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id);
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<IEnumerable<Tag>> SearchAsync(string searchTerm);
    Task<Tag?> GetByNameAsync(string name);
    Task AddAsync(Tag tag);
    Task UpdateAsync(Tag tag);
    Task DeleteAsync(Guid id);
}