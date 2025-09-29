using FitnessApp.Modules.Content.Application.Interfaces;
using FitnessApp.Modules.Content.Domain.Entities;
using FitnessApp.Modules.Content.Infrastructure.Storage;
using FitnessApp.Modules.Content.Infrastructure.Persistence;
using FitnessApp.Modules.Content.Infrastructure.Workers;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Content.Application.Services;

public class MediaAssetService : IMediaAssetService
{
    private readonly IMediaAssetRepository _repository;
    private readonly IStorageService _storage;
    private readonly ContentDbContext _dbContext;
    private readonly ITranscodeQueue? _transcodeQueue;
    private readonly ILogger<MediaAssetService> _logger;

    public MediaAssetService(IMediaAssetRepository repository, IStorageService storage, ContentDbContext dbContext, ILogger<MediaAssetService> logger, ITranscodeQueue? transcodeQueue = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _transcodeQueue = transcodeQueue;
    }

    public async Task<Guid> UploadAsync(Stream fileStream, string fileName, string contentType, Guid exerciseId, string? description)
    {
        _logger.LogInformation("Starting media upload for exercise {ExerciseId}. FileName: {FileName}, ContentType: {ContentType}", 
            exerciseId, fileName, contentType);

        var key = $"exercises/{exerciseId}/{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        await _storage.PutObjectAsync(fileStream, key, contentType);
        var url = _storage.GetObjectUrl(key);

        var asset = new MediaAsset(key, url, contentType, description, contentType);
        await _repository.AddAsync(asset);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Media asset created successfully. AssetId: {AssetId}, StorageKey: {StorageKey}", 
            asset.Id, key);

        try
        {
            _dbContext.ExerciseMediaAssets.Add(new ExerciseMediaAsset { ExerciseId = exerciseId, MediaAssetId = asset.Id });
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Exercise-media association created. ExerciseId: {ExerciseId}, AssetId: {AssetId}", 
                exerciseId, asset.Id);
        }
        catch
        {
            // best-effort: ignore failures when persisting the join entry
            _logger.LogWarning("Failed to create exercise-media association. ExerciseId: {ExerciseId}, AssetId: {AssetId}", 
                exerciseId, asset.Id);
        }

        if (_transcodeQueue != null)
        {
            await _transcodeQueue.EnqueueAsync(new TranscodeRequest(asset.Id, asset.Key, "mp4"));
            _logger.LogInformation("Transcoding queued for media asset. AssetId: {AssetId}", asset.Id);
        }

        return asset.Id;
    }

    public async Task<Stream> DownloadAsync(Guid id)
    {
        _logger.LogInformation("Starting media download. AssetId: {AssetId}", id);
        
        var asset = await _repository.GetByIdAsync(id) ?? throw new ArgumentException("Asset not found");
        
        _logger.LogInformation("Media download successful. AssetId: {AssetId}, StorageKey: {StorageKey}", 
            id, asset.Key);
            
        return await _storage.GetObjectAsync(asset.Key);
    }

    public async Task<IEnumerable<MediaAsset>> GetByExerciseIdAsync(Guid exerciseId)
    {
        _logger.LogInformation("Retrieving media assets for exercise. ExerciseId: {ExerciseId}", exerciseId);
        
        var assets = await _repository.GetByExerciseIdAsync(exerciseId);
        var assetsList = assets.ToList();
        
        _logger.LogInformation("Retrieved {AssetCount} media assets for exercise. ExerciseId: {ExerciseId}", 
            assetsList.Count, exerciseId);
            
        return assetsList;
    }

    //delete
    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Starting media asset deletion. AssetId: {AssetId}", id);
        
        var asset = await _repository.GetByIdAsync(id);
        if (asset == null) 
        {
            _logger.LogWarning("Media asset not found for deletion. AssetId: {AssetId}", id);
            throw new ArgumentException("Asset not found");
        }
        
        await _storage.DeleteObjectAsync(asset.Key);
        await _repository.DeleteAsync(asset);
        await _repository.SaveChangesAsync();
        
        _logger.LogInformation("Media asset deleted successfully. AssetId: {AssetId}, StorageKey: {StorageKey}", 
            id, asset.Key);
    }
}
