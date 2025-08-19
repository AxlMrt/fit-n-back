using FitnessApp.Modules.Users.Domain.Entities;

namespace FitnessApp.Modules.Users.Domain.Repositories;
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> UserNameExistsAsync(string userName);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid userId);

    // Preference-specific operations: fetch, upsert (add or update), and delete by key
    Task<IReadOnlyCollection<Preference>> GetPreferencesAsync(Guid userId);
    Task UpsertPreferencesAsync(Guid userId, IEnumerable<Preference> preferences);
    Task DeletePreferencesAsync(Guid userId, IEnumerable<(string Category, string Key)> keys);

    Task SaveChangesAsync();
}