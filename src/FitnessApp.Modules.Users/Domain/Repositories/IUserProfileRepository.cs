using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Repositories;

/// <summary>
/// Repository interface for UserProfile aggregate root.
/// Handles only profile data, not authentication data.
/// </summary>
public interface IUserProfileRepository
{
    // Basic CRUD operations
    Task<UserProfile?> GetByUserIdAsync(Guid userId);
    Task<UserProfile> AddAsync(UserProfile profile);
    Task<UserProfile> UpdateAsync(UserProfile profile);
    Task<bool> DeleteAsync(Guid userId);

    // Query operations
    Task<IEnumerable<UserProfile>> GetAllAsync();
    Task<(IEnumerable<UserProfile> Profiles, int TotalCount)> GetPagedAsync(
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
        int pageSize = 20);

    // Existence checks
    Task<bool> ProfileExistsAsync(Guid userId);

    // Preference operations (managed through UserProfile aggregate)
    Task<IReadOnlyCollection<Preference>> GetPreferencesAsync(Guid userId);

    // Specialized queries
    Task<IEnumerable<UserProfile>> GetActiveProfilesAsync();
    Task<IEnumerable<UserProfile>> GetProfilesWithExpiredSubscriptionsAsync();
    Task<IEnumerable<UserProfile>> GetNewProfilesAsync(DateTime since);
    Task<int> GetTotalProfilesCountAsync();
    Task<Dictionary<SubscriptionLevel, int>> GetSubscriptionDistributionAsync();
    Task<Dictionary<FitnessLevel, int>> GetFitnessLevelDistributionAsync();
    Task<Dictionary<Gender, int>> GetGenderDistributionAsync();
}