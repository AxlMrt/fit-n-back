using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Application.Mapping;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.SharedKernel.DTOs.UserProfile.Requests;
using UserProfileResponses = FitnessApp.SharedKernel.DTOs.UserProfile.Responses;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Users.Application.Services;

/// <summary>
/// Service for managing user profiles.
/// Handles only profile-related operations, not authentication.
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(IUserProfileRepository repository, ILogger<UserProfileService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<UserProfileResponses.UserProfileDto?> GetByUserIdAsync(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        return profile?.MapToDto();
    }

    public async Task<UserProfileResponses.UserProfileListDto?> GetListDtoByUserIdAsync(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        return profile?.MapToListDto();
    }

    public async Task<UserProfileResponses.UserProfileDto> CreateProfileAsync(CreateUserProfileRequest request)
    {
        // Check if profile already exists
        if (await _repository.GetByUserIdAsync(request.UserId) != null)
        {
            throw new InvalidOperationException($"Profile already exists for user {request.UserId}");
        }

        var profile = new UserProfile(request.UserId);
        
        // Update profile information using existing methods
        if (!string.IsNullOrEmpty(request.FirstName) || !string.IsNullOrEmpty(request.LastName))
        {
            var fullName = FullName.Create(request.FirstName, request.LastName);
            var dateOfBirth = request.DateOfBirth.HasValue ? DateOfBirth.Create(request.DateOfBirth.Value) : null;
            profile.UpdatePersonalInfo(fullName, dateOfBirth, request.Gender);
        }

        if (request.HeightInMeters.HasValue || request.WeightInKilograms.HasValue)
        {
            var heightCm = request.HeightInMeters.HasValue ? (int?)(request.HeightInMeters.Value * 100) : null;
            var weightKg = request.WeightInKilograms.HasValue ? (int?)request.WeightInKilograms.Value : null;
            var measurements = PhysicalMeasurements.Create(heightCm, weightKg);
            profile.UpdatePhysicalMeasurements(measurements);
        }

        if (request.FitnessLevel.HasValue)
        {
            profile.UpdateFitnessProfile(request.FitnessLevel, null);
        }

        await _repository.AddAsync(profile);

        _logger.LogInformation("Created profile for user {UserId}", request.UserId);
        return profile.MapToDto();
    }

    public async Task<UserProfileResponses.UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            throw new InvalidOperationException($"Profile not found for user {userId}");
        }

        // Update personal information if provided
        if (request.FirstName != null || request.LastName != null || request.DateOfBirth.HasValue || request.Gender.HasValue)
        {
            var firstName = request.FirstName ?? profile.Name?.FirstName;
            var lastName = request.LastName ?? profile.Name?.LastName;
            var fullName = !string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName) 
                ? FullName.Create(firstName, lastName) 
                : profile.Name;
            
            var dateOfBirth = request.DateOfBirth.HasValue 
                ? DateOfBirth.Create(request.DateOfBirth.Value)
                : profile.DateOfBirth;
            
            var gender = request.Gender ?? profile.Gender;
            
            profile.UpdatePersonalInfo(fullName, dateOfBirth, gender);
        }

        // Update physical measurements if provided
        if (request.HeightInMeters.HasValue || request.WeightInKilograms.HasValue)
        {
            var currentHeightCm = profile.PhysicalMeasurements?.HeightCm;
            var currentWeightKg = profile.PhysicalMeasurements?.WeightKg;
            
            var heightCm = request.HeightInMeters.HasValue ? (int?)(request.HeightInMeters.Value * 100) : currentHeightCm;
            var weightKg = request.WeightInKilograms.HasValue ? (int?)request.WeightInKilograms.Value : currentWeightKg;
            
            var measurements = PhysicalMeasurements.Create(heightCm, weightKg);
            profile.UpdatePhysicalMeasurements(measurements);
        }

        // Update fitness profile if provided
        if (request.FitnessLevel.HasValue)
        {
            profile.UpdateFitnessProfile(request.FitnessLevel, profile.PrimaryFitnessGoal);
        }

        await _repository.UpdateAsync(profile);

        _logger.LogInformation("Updated profile for user {UserId}", userId);
        return profile.MapToDto();
    }

    public async Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        if (profile == null)
        {
            throw new InvalidOperationException($"Profile not found for user {userId}");
        }

        // Update preferences based on available properties
        if (request.NotificationsEnabled.HasValue)
        {
            profile.AddOrUpdatePreference("Notifications", "Enabled", request.NotificationsEnabled.Value.ToString());
        }

        if (!string.IsNullOrEmpty(request.PreferredLanguage))
        {
            profile.AddOrUpdatePreference("Localization", "Language", request.PreferredLanguage);
        }

        if (!string.IsNullOrEmpty(request.TimeZone))
        {
            profile.AddOrUpdatePreference("Localization", "TimeZone", request.TimeZone);
        }

        if (request.PublicProfile.HasValue)
        {
            profile.AddOrUpdatePreference("Privacy", "PublicProfile", request.PublicProfile.Value.ToString());
        }

        if (request.ShowAgePublicly.HasValue)
        {
            profile.AddOrUpdatePreference("Privacy", "ShowAgePublicly", request.ShowAgePublicly.Value.ToString());
        }

        if (request.ShowWeightProgressPublicly.HasValue)
        {
            profile.AddOrUpdatePreference("Privacy", "ShowWeightProgressPublicly", request.ShowWeightProgressPublicly.Value.ToString());
        }

        await _repository.UpdateAsync(profile);
        _logger.LogInformation("Updated preferences for user {UserId}", userId);
    }

    public async Task<PagedResult<UserProfileResponses.UserProfileDto>> GetProfilesAsync(UserProfileQueryRequest request)
    {
        // Simplified implementation - returning empty result for now
        await Task.CompletedTask; // Make method truly async
        var profileDtos = new List<UserProfileResponses.UserProfileDto>();
        
        return new PagedResult<UserProfileResponses.UserProfileDto>(
            profileDtos,
            0,
            request.Page,
            request.PageSize,
            0
        );
    }

    public async Task<UserProfileResponses.UserProfileStatsDto> GetProfileStatsAsync()
    {
        // Simplified implementation - returning empty stats for now
        await Task.CompletedTask; // Make method truly async
        
        return new UserProfileResponses.UserProfileStatsDto(
            0, // TotalProfiles
            0, // ActiveProfiles
            0, // FreeUsers
            0, // PremiumUsers
            0, // CoachUsers
            new Dictionary<FitnessLevel, int>(), // FitnessLevelDistribution
            new Dictionary<Gender, int>(), // GenderDistribution
            new Dictionary<SubscriptionLevel, int>() // SubscriptionDistribution
        );
    }

    public async Task<bool> ProfileExistsAsync(Guid userId)
    {
        return await _repository.GetByUserIdAsync(userId) != null;
    }

    public async Task<bool> HasCompletedProfileAsync(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        return profile?.HasCompletedProfile() ?? false;
    }

    public async Task<bool> CanAccessPremiumFeaturesAsync(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        return profile?.CanAccessPremiumFeatures() ?? false;
    }
}
