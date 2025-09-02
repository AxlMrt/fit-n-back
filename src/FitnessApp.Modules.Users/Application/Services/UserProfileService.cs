using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Application.Mapping;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.SharedKernel.DTOs.Users.Requests;
using FitnessApp.SharedKernel.DTOs.Users.Responses;
using FitnessApp.SharedKernel.Enums;
using FitnessApp.SharedKernel.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Users.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IValidationService _validationService;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IValidationService validationService,
        ILogger<UserProfileService> logger)
    {
        _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user profile for user {UserId}", userId);

        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return profile?.ToResponse();
    }

    public async Task<UserProfileSummaryResponse?> GetUserProfileSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting user profile summary for user {UserId}", userId);

        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return profile?.ToSummaryResponse();
    }

    public async Task<UserProfileResponse> CreateUserProfileAsync(Guid userId, CreateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user profile for user {UserId}", userId);

        // Validate request
        await _validationService.ValidateAsync(request);

        // Check if profile already exists
        var existingProfile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingProfile != null)
        {
            throw new InvalidOperationException("User profile already exists");
        }

        // Create profile
        var profile = new UserProfile(userId);
        
        // Set personal information
        var fullName = UserProfileMappingExtensions.ToFullName(request.FirstName, request.LastName);
        var dateOfBirth = DateOfBirth.Create(request.DateOfBirth);
        profile.UpdatePersonalInfo(fullName, dateOfBirth, request.Gender);

        // Set physical measurements
        var measurements = UserProfileMappingExtensions.ToPhysicalMeasurements(request.HeightCm, request.WeightKg);
        profile.UpdatePhysicalMeasurements(measurements);

        // Set fitness profile
        profile.UpdateFitnessProfile(request.FitnessLevel, request.PrimaryFitnessGoal);

        // Save profile
        await _userProfileRepository.AddAsync(profile, cancellationToken);
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User profile created successfully for user {UserId}", userId);
        
        return profile.ToResponse();
    }

    public async Task<UserProfileResponse> UpdatePersonalInfoAsync(Guid userId, UpdatePersonalInfoRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating personal info for user {UserId}", userId);

        // Validate request
        await _validationService.ValidateAsync(request);

        // Get existing profile
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        // Update personal information
        var fullName = request.FirstName != null || request.LastName != null
            ? UserProfileMappingExtensions.ToFullName(request.FirstName, request.LastName)
            : null;
        
        var dateOfBirth = request.DateOfBirth.HasValue
            ? DateOfBirth.Create(request.DateOfBirth.Value)
            : null;

        profile.UpdatePersonalInfo(fullName, dateOfBirth, request.Gender);

        // Save changes
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Personal info updated successfully for user {UserId}", userId);
        
        return profile.ToResponse();
    }

    public async Task<UserProfileResponse> UpdatePhysicalMeasurementsAsync(Guid userId, UpdatePhysicalMeasurementsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating physical measurements for user {UserId}", userId);

        // Validate request
        await _validationService.ValidateAsync(request);

        // Get existing profile
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        // Update measurements
        var measurements = UserProfileMappingExtensions.ToPhysicalMeasurements(request.HeightCm, request.WeightKg);
        profile.UpdatePhysicalMeasurements(measurements);

        // Save changes
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Physical measurements updated successfully for user {UserId}", userId);
        
        return profile.ToResponse();
    }

    public async Task<UserProfileResponse> UpdateFitnessProfileAsync(Guid userId, UpdateFitnessProfileRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating fitness profile for user {UserId}", userId);

        // Validate request
        await _validationService.ValidateAsync(request);

        // Get existing profile
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        // Update fitness profile
        profile.UpdateFitnessProfile(request.FitnessLevel, request.PrimaryFitnessGoal);

        // Save changes
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fitness profile updated successfully for user {UserId}", userId);
        
        return profile.ToResponse();
    }

    public async Task<ProfileOperationResponse> DeleteUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user profile for user {UserId}", userId);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        _userProfileRepository.Remove(profile);
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User profile deleted successfully for user {UserId}", userId);
        
        return new ProfileOperationResponse("Profile deleted successfully");
    }

    public async Task<SubscriptionResponse?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting subscription for user {UserId}", userId);

        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return profile?.Subscription?.ToResponse();
    }

    public async Task<SubscriptionResponse> UpdateSubscriptionAsync(Guid userId, UpdateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating subscription for user {UserId}", userId);

        // Validate request
        await _validationService.ValidateAsync(request);

        // Get existing profile
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        // Create or update subscription
        var subscription = new Subscription(profile, request.Level, request.StartDate, request.EndDate);
        profile.UpdateSubscription(subscription);

        // Save changes
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription updated successfully for user {UserId}", userId);
        
        return subscription.ToResponse();
    }

    public async Task<ProfileOperationResponse> CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling subscription for user {UserId}", userId);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        if (profile.Subscription == null)
        {
            throw new InvalidOperationException("No active subscription found");
        }

        profile.Subscription.Cancel();
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription cancelled successfully for user {UserId}", userId);
        
        return new ProfileOperationResponse("Subscription cancelled successfully");
    }

    public async Task<SubscriptionResponse> RenewSubscriptionAsync(Guid userId, DateTime newEndDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Renewing subscription for user {UserId}", userId);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        if (profile.Subscription == null)
        {
            throw new InvalidOperationException("No subscription found to renew");
        }

        profile.Subscription.Renew(newEndDate);
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Subscription renewed successfully for user {UserId}", userId);
        
        return profile.Subscription.ToResponse();
    }

    public async Task<UserPreferencesResponse> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting preferences for user {UserId}", userId);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);
        
        return profile.ToPreferencesResponse();
    }

    public async Task<UserPreferencesResponse> GetUserPreferencesByCategoryAsync(Guid userId, PreferenceCategory category, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting preferences for user {UserId} in category {Category}", userId, category);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);
        
        return profile.ToPreferencesByCategoryResponse(category);
    }

    public async Task<PreferenceResponse> CreateOrUpdatePreferenceAsync(Guid userId, CreateOrUpdatePreferenceRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating/updating preference for user {UserId}", userId);

        // Validate request
        await _validationService.ValidateAsync(request);

        // Use repository method to handle preference update with proper EF tracking
        var updatedProfile = await _userProfileRepository.UpdatePreferenceAsync(
            userId, 
            request.Category, 
            request.Key, 
            request.Value ?? string.Empty, 
            cancellationToken);
        
        // Find the created/updated preference
        var preference = updatedProfile.Preferences
            .FirstOrDefault(p => p.Category == request.Category && p.Key == request.Key);

        if (preference == null)
        {
            throw new InvalidOperationException($"Failed to create/update preference {request.Category}.{request.Key}");
        }

        _logger.LogInformation("Preference created/updated successfully for user {UserId}", userId);
        
        return preference.ToResponse();
    }

    public async Task<UserPreferencesResponse> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating multiple preferences for user {UserId}", userId);

        // Get existing profile
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        // Update preferences
        foreach (var categoryGroup in request.Preferences)
        {
            var category = categoryGroup.Key;
            foreach (var preference in categoryGroup.Value)
            {
                profile.AddOrUpdatePreference(category, preference.Key, preference.Value ?? string.Empty);
            }
        }

        // Save changes
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Multiple preferences updated successfully for user {UserId}", userId);
        
        return profile.ToPreferencesResponse();
    }

    public async Task<ProfileOperationResponse> DeletePreferenceAsync(Guid userId, PreferenceCategory category, string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting preference for user {UserId} in category {Category} with key {Key}", userId, category, key);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        profile.RemovePreference(category, key);
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Preference deleted successfully for user {UserId}", userId);
        
        return new ProfileOperationResponse("Preference deleted successfully");
    }

    public async Task<ProfileOperationResponse> ClearPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Clearing all preferences for user {UserId}", userId);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        profile.ClearPreferences();
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("All preferences cleared successfully for user {UserId}", userId);
        
        return new ProfileOperationResponse("All preferences cleared successfully");
    }

    public async Task<bool> UserProfileExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userProfileRepository.ExistsAsync(userId, cancellationToken);
    }

    private async Task<UserProfile> GetProfileOrThrowAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        if (profile == null)
        {
            throw new InvalidOperationException($"User profile not found for user {userId}");
        }

        return profile;
    }
}