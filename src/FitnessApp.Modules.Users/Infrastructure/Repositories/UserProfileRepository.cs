using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Users.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for UserProfile aggregate.
/// Handles only profile data, not authentication data.
/// </summary>
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
            .Include(p => p.Preferences)
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Preferences)
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.UserId == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles.AnyAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Preferences)
            .Include(p => p.Subscription)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserProfiles.AddAsync(userProfile, cancellationToken);
    }

    public void Update(UserProfile userProfile)
    {
        _dbContext.UserProfiles.Update(userProfile);
    }

    public void Remove(UserProfile userProfile)
    {
        _dbContext.UserProfiles.Remove(userProfile);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserProfile>> FindByFitnessLevelAsync(string fitnessLevel, CancellationToken cancellationToken = default)
    {
        if (Enum.TryParse<FitnessLevel>(fitnessLevel, true, out var level))
        {
            return await _dbContext.UserProfiles
                .Where(p => p.FitnessLevel == level)
                .Include(p => p.Preferences)
                .Include(p => p.Subscription)
                .ToListAsync(cancellationToken);
        }
        return Enumerable.Empty<UserProfile>();
    }

    public async Task<IEnumerable<UserProfile>> FindBySubscriptionLevelAsync(string subscriptionLevel, CancellationToken cancellationToken = default)
    {
        if (Enum.TryParse<SubscriptionLevel>(subscriptionLevel, true, out var level))
        {
            return await _dbContext.UserProfiles
                .Where(p => p.Subscription != null && p.Subscription.Level == level)
                .Include(p => p.Preferences)
                .Include(p => p.Subscription)
                .ToListAsync(cancellationToken);
        }
        return Enumerable.Empty<UserProfile>();
    }

    // Legacy methods kept for backward compatibility
    public async Task<UserProfile> AddAsync(UserProfile profile)
    {
        _dbContext.UserProfiles.Add(profile);
        await _dbContext.SaveChangesAsync();
        return profile;
    }

    public async Task<UserProfile> UpdateAsync(UserProfile profile)
    {
        var entry = _dbContext.Entry(profile);
        if (entry.State == EntityState.Detached)
        {
            _dbContext.UserProfiles.Attach(profile);
            entry.State = EntityState.Modified;
        }
        
        await _dbContext.SaveChangesAsync();
        return profile;
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var profile = await GetByUserIdAsync(userId);
        if (profile == null) return false;

        _dbContext.UserProfiles.Remove(profile);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    // Alias for compatibility - delegates to ExistsAsync
    public async Task<bool> ProfileExistsAsync(Guid userId)
    {
        return await ExistsAsync(userId);
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        return await _dbContext.UserProfiles
            .Include(p => p.Preferences)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IEnumerable<UserProfile> Profiles, int TotalCount)> GetPagedAsync(
        string? nameFilter = null,
        Gender? genderFilter = null,
        FitnessLevel? fitnessLevelFilter = null,
        SubscriptionLevel? subscriptionFilter = null,
        int? minAge = null,
        int? maxAge = null,
        bool publicProfilesOnly = false,
        string sortBy = "CreatedAt",
        bool sortDescending = true,
        int page = 1,
        int pageSize = 20)
    {
        var query = _dbContext.UserProfiles.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(p => 
                (p.Name.FirstName != null && p.Name.FirstName.Contains(nameFilter)) ||
                (p.Name.LastName != null && p.Name.LastName.Contains(nameFilter)) ||
                p.Name.DisplayName.Contains(nameFilter));
        }

        if (genderFilter.HasValue)
            query = query.Where(p => p.Gender == genderFilter.Value);

        if (fitnessLevelFilter.HasValue)
            query = query.Where(p => p.FitnessLevel == fitnessLevelFilter.Value);

        if (subscriptionFilter.HasValue)
            query = query.Where(p => p.Subscription != null && p.Subscription.Level == subscriptionFilter.Value);

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortDescending 
                ? query.OrderByDescending(p => p.Name.DisplayName)
                : query.OrderBy(p => p.Name.DisplayName),
            "createdat" => sortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var profiles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.Preferences)
            .Include(p => p.Subscription)
            .ToListAsync();

        return (profiles, totalCount);
    }

    public async Task<IReadOnlyCollection<Preference>> GetPreferencesAsync(Guid userId)
    {
        var profile = await _dbContext.UserProfiles
            .Include(p => p.Preferences)
            .FirstOrDefaultAsync(p => p.UserId == userId);
            
        return profile?.Preferences ?? new List<Preference>().AsReadOnly();
    }

    public async Task<IEnumerable<UserProfile>> GetActiveProfilesAsync()
    {
        return await _dbContext.UserProfiles
            .Where(p => p.Subscription != null && p.Subscription.IsActive)
            .Include(p => p.Preferences)
            .Include(p => p.Subscription)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserProfile>> GetProfilesWithExpiredSubscriptionsAsync()
    {
        return await _dbContext.UserProfiles
            .Where(p => p.Subscription != null && !p.Subscription.IsActive)
            .Include(p => p.Preferences)
            .Include(p => p.Subscription)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserProfile>> GetNewProfilesAsync(DateTime since)
    {
        return await _dbContext.UserProfiles
            .Where(p => p.CreatedAt >= since)
            .Include(p => p.Preferences)
            .Include(p => p.Subscription)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalProfilesCountAsync()
    {
        return await _dbContext.UserProfiles.CountAsync();
    }

    public async Task<Dictionary<SubscriptionLevel, int>> GetSubscriptionDistributionAsync()
    {
        return await _dbContext.UserProfiles
            .Where(p => p.Subscription != null)
            .GroupBy(p => p.Subscription!.Level)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Level, x => x.Count);
    }

    public async Task<Dictionary<FitnessLevel, int>> GetFitnessLevelDistributionAsync()
    {
        return await _dbContext.UserProfiles
            .Where(p => p.FitnessLevel != null)
            .GroupBy(p => p.FitnessLevel!.Value)
            .Select(g => new { Level = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Level, x => x.Count);
    }

    public async Task<Dictionary<Gender, int>> GetGenderDistributionAsync()
    {
        return await _dbContext.UserProfiles
            .Where(p => p.Gender != null)
            .GroupBy(p => p.Gender!.Value)
            .Select(g => new { Gender = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Gender, x => x.Count);
    }

    public async Task<UserProfile> UpdatePreferenceAsync(Guid userId, PreferenceCategory category, string key, string value, CancellationToken cancellationToken = default)
    {
        var existingPreference = await _dbContext.Preferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == category && p.Key == key, cancellationToken);

        if (existingPreference != null)
        {
            existingPreference.UpdateValue(value);
        }
        else
        {
            var newPreference = new Preference(userId, category, key, value);
            await _dbContext.Preferences.AddAsync(newPreference, cancellationToken);
        }

        var profile = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        if (profile == null)
        {
            throw new InvalidOperationException($"User profile not found for user {userId}");
        }

        profile.ForceUpdateTimestamp();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.UserProfiles
            .Include(p => p.Preferences)
            .FirstAsync(p => p.UserId == userId, cancellationToken);
    }
}
