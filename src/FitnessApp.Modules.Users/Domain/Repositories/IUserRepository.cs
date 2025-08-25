using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Enums;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate root.
/// Defines data access contracts following DDD patterns.
/// </summary>
public interface IUserRepository
{
    // Basic CRUD operations
    Task<User?> GetByIdAsync(Guid userId);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> AddAsync(User user);
    Task<User> UpdateAsync(User user);
    Task<bool> DeleteAsync(Guid userId);

    // Query operations
    Task<IEnumerable<User>> GetAllAsync();
    Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
        string? emailFilter = null,
        string? nameFilter = null,
        Gender? genderFilter = null,
        FitnessLevel? fitnessLevelFilter = null,
        bool? isActiveFilter = null,
        string sortBy = "CreatedAt",
        bool sortDescending = true,
        int page = 1,
        int pageSize = 20);

    // Existence checks
    Task<bool> ExistsWithEmailAsync(string email);
    Task<bool> ExistsWithUsernameAsync(string username);
    Task<bool> ExistsAsync(Guid userId);

    // Preference operations (managed through User aggregate)
    Task<IReadOnlyCollection<Preference>> GetPreferencesAsync(Guid userId);
    Task UpsertPreferencesAsync(Guid userId, IEnumerable<Preference> preferences);

    // Specialized queries
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<IEnumerable<User>> GetUsersWithExpiredSubscriptionsAsync();
    Task<IEnumerable<User>> GetNewUsersAsync(DateTime since);
    Task<int> GetTotalUsersCountAsync();
}