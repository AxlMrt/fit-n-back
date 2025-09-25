using AutoMapper;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Domain.Services;
using FitnessApp.SharedKernel.DTOs.Users.Requests;
using FitnessApp.SharedKernel.DTOs.Users.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Services;

public class UserPreferenceService : IUserPreferenceService
{
    private readonly IUserPreferenceRepository _preferenceRepository;
    private readonly UserPreferenceDomainService _domainService;
    private readonly IMapper _mapper;

    public UserPreferenceService(
        IUserPreferenceRepository preferenceRepository,
        UserPreferenceDomainService domainService,
        IMapper mapper)
    {
        _preferenceRepository = preferenceRepository ?? throw new ArgumentNullException(nameof(preferenceRepository));
        _domainService = domainService ?? throw new ArgumentNullException(nameof(domainService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserPreferencesResponse> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var preferences = await _preferenceRepository.GetPreferencesAsync(userId, cancellationToken);
        
        var groupedPreferences = preferences
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(p => p.Key, p => (string?)p.Value) as IDictionary<string, string?>
            );

        return new UserPreferencesResponse(userId, groupedPreferences);
    }

    public async Task<UserPreferencesResponse> GetUserPreferencesByCategoryAsync(Guid userId, PreferenceCategory category, CancellationToken cancellationToken = default)
    {
        var preferences = await _preferenceRepository.GetPreferencesByCategoryAsync(userId, category, cancellationToken);
        
        var categoryPreferences = preferences.ToDictionary(p => p.Key, p => (string?)p.Value);
        var result = new Dictionary<PreferenceCategory, IDictionary<string, string?>>
        {
            { category, categoryPreferences }
        } as IDictionary<PreferenceCategory, IDictionary<string, string?>>;

        return new UserPreferencesResponse(userId, result);
    }

    public async Task<PreferenceResponse> CreateOrUpdatePreferenceAsync(Guid userId, CreateOrUpdatePreferenceRequest request, CancellationToken cancellationToken = default)
    {
        if (!_domainService.IsValidPreferenceValue(request.Category, request.Key, request.Value ?? string.Empty))
        {
            throw new ArgumentException($"Invalid preference value for {request.Category}.{request.Key}");
        }

        var newPreference = _domainService.AddOrUpdatePreference(userId, request.Category, request.Key, request.Value ?? string.Empty);
        
        var savedPreference = await _preferenceRepository.AddOrUpdatePreferenceAsync(newPreference, cancellationToken);

        return _mapper.Map<PreferenceResponse>(savedPreference);
    }

    public async Task<UserPreferencesResponse> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default)
    {
        var allPreferences = new List<Preference>();

        foreach (var categoryGroup in request.Preferences)
        {
            var category = categoryGroup.Key;
            foreach (var prefRequest in categoryGroup.Value)
            {
                if (!_domainService.IsValidPreferenceValue(category, prefRequest.Key, prefRequest.Value ?? string.Empty))
                {
                    throw new ArgumentException($"Invalid preference value for {category}.{prefRequest.Key}");
                }

                var preference = _domainService.AddOrUpdatePreference(userId, category, prefRequest.Key, prefRequest.Value ?? string.Empty);
                var savedPreference = await _preferenceRepository.AddOrUpdatePreferenceAsync(preference, cancellationToken);
                allPreferences.Add(savedPreference);
            }
        }

        return await GetUserPreferencesAsync(userId, cancellationToken);
    }

    public async Task<ProfileOperationResponse> DeletePreferenceAsync(Guid userId, PreferenceCategory category, string key, CancellationToken cancellationToken = default)
    {
        var success = await _preferenceRepository.RemovePreferenceAsync(userId, category, key, cancellationToken);

        if (!success)
        {
            return new ProfileOperationResponse("Preference not found");
        }
        
        return new ProfileOperationResponse("Preference deleted successfully");
    }

    public async Task<ProfileOperationResponse> ClearPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var deletedCount = await _preferenceRepository.RemoveAllPreferencesAsync(userId, cancellationToken);

        return new ProfileOperationResponse($"Cleared {deletedCount} preferences successfully");
    }
}
