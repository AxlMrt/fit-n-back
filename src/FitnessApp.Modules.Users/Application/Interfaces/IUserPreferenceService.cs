using FitnessApp.SharedKernel.DTOs.Users.Requests;
using FitnessApp.SharedKernel.DTOs.Users.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Interfaces;

public interface IUserPreferenceService
{
    Task<UserPreferencesResponse> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserPreferencesResponse> GetUserPreferencesByCategoryAsync(Guid userId, PreferenceCategory category, CancellationToken cancellationToken = default);

    Task<PreferenceResponse> CreateOrUpdatePreferenceAsync(Guid userId, CreateOrUpdatePreferenceRequest request, CancellationToken cancellationToken = default);

    Task<UserPreferencesResponse> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default);

    Task<ProfileOperationResponse> DeletePreferenceAsync(Guid userId, PreferenceCategory category, string key, CancellationToken cancellationToken = default);

    Task<ProfileOperationResponse> ClearPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}
