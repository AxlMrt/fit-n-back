using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Repositories;

public interface IUserPreferenceRepository
{
    Task<IReadOnlyList<Preference>> GetPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Preference>> GetPreferencesByCategoryAsync(Guid userId, PreferenceCategory category, CancellationToken cancellationToken = default);
    Task<Preference?> GetPreferenceAsync(Guid userId, PreferenceCategory category, string key, CancellationToken cancellationToken = default);
    Task<Preference> AddOrUpdatePreferenceAsync(Preference preference, CancellationToken cancellationToken = default);
    Task<bool> RemovePreferenceAsync(Guid userId, PreferenceCategory category, string key, CancellationToken cancellationToken = default);
    Task<int> RemoveAllPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}
