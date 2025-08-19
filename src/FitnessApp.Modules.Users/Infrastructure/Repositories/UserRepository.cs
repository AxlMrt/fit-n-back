using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Users.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _dbContext;

    public UserRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _dbContext.Users
            .Include(u => u.Profile)
            .Include(u => u.Subscription)
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .Include(u => u.Profile)
            .Include(u => u.Subscription)
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> UserNameExistsAsync(string userName)
    {
        return await _dbContext.Users.AnyAsync(u => u.UserName == userName);
    }

    public async Task<User> CreateAsync(User user)
    {
        _dbContext.Users.Add(user);
        await SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        var entry = _dbContext.Entry(user);
        // If the entity is not being tracked by the context, attach it and mark for update.
        // If it is already tracked (e.g. loaded with GetByIdAsync), its changes are tracked
        // and we should not force the state to Modified to avoid incorrect updates
        // on related entities which can lead to concurrency exceptions.
        if (entry.State == EntityState.Detached)
        {
            _dbContext.Users.Update(user);
        }

        await SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        
        if (user == null)
        {
            return false;
        }

        _dbContext.Users.Remove(user);
        await SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyCollection<Preference>> GetPreferencesAsync(Guid userId)
    {
        var prefs = await _dbContext.Preferences
            .Where(p => p.UserId == userId)
            .ToListAsync();
        return prefs;
    }

    public async Task UpsertPreferencesAsync(Guid userId, IEnumerable<Preference> preferences)
    {
        foreach (var pref in preferences)
        {
            var existing = await _dbContext.Preferences
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == pref.Category && p.Key == pref.Key);

            if (existing == null)
            {
                // Ensure the UserId is set to the provided userId (defensive)
                var toAdd = new Preference(userId, pref.Category, pref.Key, pref.Value);
                _dbContext.Preferences.Add(toAdd);
            }
            else
            {
                existing.UpdateValue(pref.Value);
                _dbContext.Preferences.Update(existing);
            }
        }

        await SaveChangesAsync();
    }

    public async Task DeletePreferencesAsync(Guid userId, IEnumerable<(string Category, string Key)> keys)
    {
        foreach (var k in keys)
        {
            var existing = await _dbContext.Preferences
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Category == k.Category && p.Key == k.Key);
            if (existing != null)
            {
                _dbContext.Preferences.Remove(existing);
            }
        }

        await SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}