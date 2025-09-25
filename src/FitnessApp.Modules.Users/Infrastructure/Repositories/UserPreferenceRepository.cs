using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Users.Infrastructure.Repositories;

public class UserPreferenceRepository : IUserPreferenceRepository
{
    private readonly UsersDbContext _context;

    public UserPreferenceRepository(UsersDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<Preference>> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Preferences
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Preference>> GetPreferencesByCategoryAsync(Guid userId, PreferenceCategory category, CancellationToken cancellationToken = default)
    {
        return await _context.Preferences
            .Where(p => p.UserId == userId && p.Category == category)
            .OrderBy(p => p.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<Preference?> GetPreferenceAsync(Guid userId, PreferenceCategory category, string key, CancellationToken cancellationToken = default)
    {
        return await _context.Preferences
            .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == category && p.Key == key, cancellationToken);
    }

    public async Task<Preference> AddOrUpdatePreferenceAsync(Preference preference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preference);

        var existingPreference = await GetPreferenceAsync(
            preference.UserId, 
            preference.Category, 
            preference.Key, 
            cancellationToken);

        if (existingPreference != null)
        {
            existingPreference.UpdateValue(preference.Value);
            await _context.SaveChangesAsync(cancellationToken);
            return existingPreference;
        }
        else
        {
            _context.Preferences.Add(preference);
            await _context.SaveChangesAsync(cancellationToken);
            return preference;
        }
    }

    public async Task<bool> RemovePreferenceAsync(Guid userId, PreferenceCategory category, string key, CancellationToken cancellationToken = default)
    {
        var preference = await GetPreferenceAsync(userId, category, key, cancellationToken);
        
        if (preference == null)
            return false;

        _context.Preferences.Remove(preference);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> RemoveAllPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preferences = await _context.Preferences
            .Where(p => p.UserId == userId)
            .ToListAsync(cancellationToken);

        var count = preferences.Count;
        _context.Preferences.RemoveRange(preferences);
        await _context.SaveChangesAsync(cancellationToken);
        
        return count;
    }
}
