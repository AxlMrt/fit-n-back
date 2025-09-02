using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Repositories;

/// <summary>
/// Repository interface for user profile operations.
/// </summary>
public interface IUserProfileRepository
{
    // Read operations
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserProfile>> GetAllAsync(CancellationToken cancellationToken = default);

    // Write operations
    Task AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    void Update(UserProfile userProfile);
    void Remove(UserProfile userProfile);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    // Query operations
    Task<IEnumerable<UserProfile>> FindByFitnessLevelAsync(string fitnessLevel, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserProfile>> FindBySubscriptionLevelAsync(string subscriptionLevel, CancellationToken cancellationToken = default);
    
    // Preference operations
    Task<UserProfile> UpdatePreferenceAsync(Guid userId, PreferenceCategory category, string key, string value, CancellationToken cancellationToken = default);
}