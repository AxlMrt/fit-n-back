using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Enums;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Enums;
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
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbContext.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Email.Value == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbContext.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Username.Value == username);
    }

    public async Task<User> AddAsync(User user)
    {
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        // Simple approach: if the user is already tracked, just save
        var entry = _dbContext.Entry(user);
        if (entry.State == EntityState.Detached)
        {
            // Attach the user and mark as modified
            _dbContext.Users.Attach(user);
            _dbContext.Entry(user).State = EntityState.Modified;
        }
        
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null) return false;
        
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _dbContext.Users
            .Include(u => u.Preferences)
            .ToListAsync();
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
        string? emailFilter = null,
        string? nameFilter = null,
        Gender? genderFilter = null,
        FitnessLevel? fitnessLevelFilter = null,
        bool? isActiveFilter = null,
        string sortBy = "CreatedAt",
        bool sortDescending = true,
        int page = 1,
        int pageSize = 20)
    {
        var query = _dbContext.Users.Include(u => u.Preferences).AsQueryable();

        // Apply basic filters
        if (!string.IsNullOrWhiteSpace(emailFilter))
            query = query.Where(u => u.Email.Value.Contains(emailFilter));

        if (!string.IsNullOrWhiteSpace(nameFilter))
            query = query.Where(u => (u.Name.FirstName != null && u.Name.FirstName.Contains(nameFilter)) || 
                                   (u.Name.LastName != null && u.Name.LastName.Contains(nameFilter)));

        if (genderFilter.HasValue)
            query = query.Where(u => u.Gender == genderFilter.Value);

        if (fitnessLevelFilter.HasValue)
            query = query.Where(u => u.FitnessLevel.HasValue && u.FitnessLevel.Value == fitnessLevelFilter.Value);

        if (isActiveFilter.HasValue)
            query = query.Where(u => isActiveFilter.Value 
                ? (!u.LockoutEnd.HasValue || u.LockoutEnd <= DateTime.UtcNow)
                : (u.LockoutEnd.HasValue && u.LockoutEnd > DateTime.UtcNow));

        var totalCount = await query.CountAsync();

        // Apply pagination
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<bool> ExistsWithEmailAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email.Value == email);
    }

    public async Task<bool> ExistsWithUsernameAsync(string username)
    {
        return await _dbContext.Users.AnyAsync(u => u.Username.Value == username);
    }

    public async Task<bool> ExistsAsync(Guid userId)
    {
        return await _dbContext.Users.AnyAsync(u => u.Id == userId);
    }

    public async Task<IReadOnlyCollection<Preference>> GetPreferencesAsync(Guid userId)
    {
        var user = await _dbContext.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Preferences ?? new List<Preference>();
    }

    public async Task UpsertPreferencesAsync(Guid userId, IEnumerable<Preference> preferences)
    {
        var user = await _dbContext.Users
            .Include(u => u.Preferences)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return;

        foreach (var pref in preferences)
        {
            var existing = user.Preferences.FirstOrDefault(p => 
                p.Category == pref.Category && p.Key == pref.Key);
                
            if (existing != null)
            {
                existing.UpdateValue(pref.Value);
            }
            else
            {
                var newPref = new Preference(userId, pref.Category, pref.Key, pref.Value);
                _dbContext.Preferences.Add(newPref);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeletePreferencesAsync(Guid userId, IEnumerable<(string Category, string Key)> keys)
    {
        var preferencesToDelete = await _dbContext.Preferences
            .Where(p => p.UserId == userId)
            .Where(p => keys.Any(k => k.Category == p.Category && k.Key == p.Key))
            .ToListAsync();

        _dbContext.Preferences.RemoveRange(preferencesToDelete);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _dbContext.Users
            .Include(u => u.Preferences)
            .Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd <= DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetUsersWithExpiredSubscriptionsAsync()
    {
        return await Task.FromResult(new List<User>());
    }

    public async Task<IEnumerable<User>> GetNewUsersAsync(DateTime since)
    {
        return await _dbContext.Users
            .Include(u => u.Preferences)
            .Where(u => u.CreatedAt >= since)
            .ToListAsync();
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _dbContext.Users.CountAsync();
    }
}
