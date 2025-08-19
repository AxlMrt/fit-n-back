using FitnessApp.Modules.Content.Domain.Entities;

namespace FitnessApp.Modules.Content.Application.Interfaces;

public interface IMediaAssetService
{
    Task<Guid> UploadAsync(Stream fileStream, string fileName, string contentType, Guid exerciseId, string? description);
    Task<Stream> DownloadAsync(Guid id);
    Task<IEnumerable<MediaAsset>> GetByExerciseIdAsync(Guid exerciseId);
    Task DeleteAsync(Guid id);
}
