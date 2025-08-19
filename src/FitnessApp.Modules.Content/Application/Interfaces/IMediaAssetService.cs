namespace FitnessApp.Modules.Content.Application.Interfaces;

public interface IMediaAssetService
{
    Task<Guid> UploadAsync(Stream fileStream, string fileName, string contentType, Guid exerciseId, string? description);
    Task<Stream> DownloadAsync(Guid id);
    Task<IEnumerable<FitnessApp.Modules.Content.Domain.Entities.MediaAsset>> GetByExerciseIdAsync(Guid exerciseId);
}
