using FitnessApp.Modules.Users.Domain.Entities;

namespace FitnessApp.Modules.Users.Domain.Repositories;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetFullProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserProfile> AddAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    Task<UserProfile> UpdateAsync(UserProfile userProfile, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IEnumerable<UserProfile>> GetActiveProfilesAsync();
    Task<IEnumerable<UserProfile>> GetProfilesByFitnessLevelAsync(int fitnessLevel);
    Task<IEnumerable<UserProfile>> GetProfilesByGenderAsync(int gender);
    Task<IEnumerable<UserProfile>> GetProfilesByAgeRangeAsync(int minAge, int maxAge);
}