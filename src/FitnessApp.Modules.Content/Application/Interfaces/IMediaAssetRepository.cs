using FitnessApp.Modules.Content.Domain.Entities;

namespace FitnessApp.Modules.Content.Application.Interfaces;

public interface IMediaAssetRepository
{
    Task<MediaAsset?> GetByIdAsync(Guid id);
    Task<MediaAsset?> GetByKeyAsync(string key);
    Task<IEnumerable<MediaAsset>> GetByExerciseIdAsync(Guid exerciseId);
    Task AddAsync(MediaAsset asset);
    Task UpdateAsync(MediaAsset asset);
    Task DeleteAsync(MediaAsset asset);
    Task SaveChangesAsync();
}
