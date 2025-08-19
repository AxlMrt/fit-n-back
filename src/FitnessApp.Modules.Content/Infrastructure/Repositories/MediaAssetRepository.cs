using FitnessApp.Modules.Content.Application.Interfaces;
using FitnessApp.Modules.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Content.Infrastructure.Persistence;

namespace FitnessApp.Modules.Content.Infrastructure.Repositories;

public class MediaAssetRepository : IMediaAssetRepository
{
    private readonly ContentDbContext _dbContext;

    public MediaAssetRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<MediaAsset?> GetByIdAsync(Guid id)
    {
        return await _dbContext.MediaAssets.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<MediaAsset?> GetByKeyAsync(string key)
    {
        return await _dbContext.MediaAssets.AsNoTracking().FirstOrDefaultAsync(a => a.Key == key);
    }

    public async Task<IEnumerable<MediaAsset>> GetByExerciseIdAsync(Guid exerciseId)
    {
        return await _dbContext.MediaAssets.AsNoTracking().Where(a => a.Key.Contains(exerciseId.ToString())).ToListAsync();
    }

    public async Task AddAsync(MediaAsset asset)
    {
        await _dbContext.MediaAssets.AddAsync(asset);
    }

    public Task UpdateAsync(MediaAsset asset)
    {
        _dbContext.Entry(asset).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(MediaAsset asset)
    {
        _dbContext.MediaAssets.Remove(asset);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
