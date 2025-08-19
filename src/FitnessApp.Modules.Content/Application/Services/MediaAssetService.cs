using FitnessApp.Modules.Content.Application.Interfaces;
using FitnessApp.Modules.Content.Domain.Entities;
using FitnessApp.Modules.Content.Infrastructure.Storage;
using FitnessApp.Modules.Content.Infrastructure.Persistence;
using FitnessApp.Modules.Content.Infrastructure.Workers;

namespace FitnessApp.Modules.Content.Application.Services;

public class MediaAssetService : IMediaAssetService
{
    private readonly IMediaAssetRepository _repository;
    private readonly IStorageService _storage;
    private readonly ContentDbContext _dbContext;
    private readonly ITranscodeQueue? _transcodeQueue;

    public MediaAssetService(IMediaAssetRepository repository, IStorageService storage, ContentDbContext dbContext, ITranscodeQueue? transcodeQueue = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _transcodeQueue = transcodeQueue;
    }

    public async Task<Guid> UploadAsync(Stream fileStream, string fileName, string contentType, Guid exerciseId, string? description)
    {
        var key = $"exercises/{exerciseId}/{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        await _storage.PutObjectAsync(fileStream, key, contentType);
        var url = _storage.GetObjectUrl(key);

        var asset = new MediaAsset(key, url, contentType, description, contentType);
        await _repository.AddAsync(asset);
        await _repository.SaveChangesAsync();

        try
        {
            _dbContext.ExerciseMediaAssets.Add(new ExerciseMediaAsset { ExerciseId = exerciseId, MediaAssetId = asset.Id });
            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            // best-effort: ignore failures when persisting the join entry
        }

        if (_transcodeQueue != null)
        {
            await _transcodeQueue.EnqueueAsync(new TranscodeRequest(asset.Id, asset.Key, "mp4"));
        }

        return asset.Id;
    }

    public async Task<Stream> DownloadAsync(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id) ?? throw new ArgumentException("Asset not found");
        return await _storage.GetObjectAsync(asset.Key);
    }

    public async Task<IEnumerable<MediaAsset>> GetByExerciseIdAsync(Guid exerciseId)
    {
        return await _repository.GetByExerciseIdAsync(exerciseId);
    }

    //delete
    public async Task DeleteAsync(Guid id)
    {
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null) throw new ArgumentException("Asset not found");
        await _storage.DeleteObjectAsync(asset.Key);
        await _repository.DeleteAsync(asset);
        await _repository.SaveChangesAsync();
    }
}
