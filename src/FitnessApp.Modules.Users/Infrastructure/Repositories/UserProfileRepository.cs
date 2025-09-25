using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Users.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UsersDbContext _dbContext;

    public UserProfileRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.UserId == id, cancellationToken);
    }

    public async Task<UserProfile> AddAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var entry = await _dbContext.UserProfiles.AddAsync(profile, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return entry.Entity;
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var entry = _dbContext.UserProfiles.Update(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return entry.Entity;
    }

    public async Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await GetByUserIdAsync(userId, cancellationToken);
        
        if (profile == null)
            return false;

        _dbContext.UserProfiles.Remove(profile);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return true;
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .AnyAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<UserProfile>> GetActiveProfilesAsync()
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Subscription)
            .Where(p => p.Subscription != null && p.Subscription.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserProfile>> GetProfilesByFitnessLevelAsync(int fitnessLevel)
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Subscription)
            .Where(p => (int?)p.FitnessLevel == fitnessLevel)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserProfile>> GetProfilesByGenderAsync(int gender)
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Subscription)
            .Where(p => (int?)p.Gender == gender)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserProfile>> GetProfilesByAgeRangeAsync(int minAge, int maxAge)
    {
        var today = DateTime.Today;
        
        var profiles = await _dbContext.UserProfiles
            .Include(p => p.Subscription)
            .Where(p => p.DateOfBirth != null)
            .ToListAsync();
            
        return profiles.Where(p => 
        {
            var age = today.Year - p.DateOfBirth!.Value.Year;
            if (p.DateOfBirth.Value.Date > today.AddYears(-age))
                age--;
            return age >= minAge && age <= maxAge;
        });
    }

    public async Task<UserProfile?> GetFullProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
