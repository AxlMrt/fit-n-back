using FitnessApp.Modules.Exercises.Domain.Entities;

namespace FitnessApp.Modules.Exercises.Domain.Repositories
{
    public interface IMediaResourceRepository
    {
        Task<MediaResource?> GetByIdAsync(Guid id);
        Task<IEnumerable<MediaResource>> GetAllAsync();
        Task<IEnumerable<MediaResource>> GetByExerciseIdAsync(Guid exerciseId);
        Task AddAsync(MediaResource mediaResource);
        Task UpdateAsync(MediaResource mediaResource);
        Task DeleteAsync(MediaResource mediaResource);
        Task SaveChangesAsync();
    }
}