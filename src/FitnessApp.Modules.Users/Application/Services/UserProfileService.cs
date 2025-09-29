using AutoMapper;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Exceptions;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.SharedKernel.DTOs.Users.Requests;
using FitnessApp.SharedKernel.DTOs.Users.Responses;
using FitnessApp.SharedKernel.Enums;
using FitnessApp.SharedKernel.Events.Users;
using FitnessApp.SharedKernel.Interfaces;
using FitnessApp.SharedKernel.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Users.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly IUserPreferenceService _userPreferenceService;
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<UserProfileService> _logger;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IUserPreferenceService userPreferenceService,
        IValidationService validationService,
        IMapper mapper,
        IMediator mediator,
        ILogger<UserProfileService> logger)
    {
        _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
        _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return profile == null ? null : await MapToResponseAsync(profile, cancellationToken);
    }

    public async Task<UserProfileSummaryResponse?> GetUserProfileSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return profile == null ? null : _mapper.Map<UserProfileSummaryResponse>(profile);
    }

    public async Task<UserProfileResponse> CreateUserProfileAsync(Guid userId, CreateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user profile for user {UserId}", userId);
        
        await _validationService.ValidateAsync(request);

        var existingProfile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingProfile != null)
        {
            _logger.LogWarning("User profile creation failed - profile already exists for user {UserId}", userId);
            throw UserDomainException.UserProfileAlreadyExists();
        }
        
        var profile = new UserProfile(userId);
        
        var fullName = CreateFullName(request.FirstName, request.LastName);
        var dateOfBirth = DateOfBirth.Create(request.DateOfBirth);
        profile.UpdatePersonalInfo(fullName, dateOfBirth, request.Gender);

        var measurements = CreatePhysicalMeasurements(request.Height, request.Weight, request.Units);
        profile.UpdatePhysicalMeasurements(measurements);

        profile.UpdateFitnessProfile(request.FitnessLevel, request.FitnessGoal);

        await _userProfileRepository.AddAsync(profile, cancellationToken);
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User profile created successfully for user {UserId} with fitness level {FitnessLevel} and goal {FitnessGoal}", 
            userId, request.FitnessLevel, request.FitnessGoal);

        // Publish event for cross-module synchronization when creating profile with physical measurements
        var measurementsEvent = new PhysicalMeasurementsUpdatedEvent(
            userId,
            request.Height,
            request.Units?.HeightUnit,
            request.Weight,
            request.Units?.WeightUnit,
            DateTime.UtcNow,
            "ProfileCreation");

        await _mediator.Publish(measurementsEvent, cancellationToken);

        return await MapToResponseAsync(profile, cancellationToken);
    }

    public async Task<UserProfileResponse> UpdatePersonalInfoAsync(Guid userId, UpdatePersonalInfoRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating personal info for user {UserId}", userId);
        
        await _validationService.ValidateAsync(request);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var fullName = request.FirstName != null || request.LastName != null
            ? CreateFullName(
                request.FirstName ?? profile.Name.FirstName,  // Preserve existing if not provided
                request.LastName ?? profile.Name.LastName)    // Preserve existing if not provided
            : null;
        
        var dateOfBirth = request.DateOfBirth.HasValue
            ? DateOfBirth.Create(request.DateOfBirth.Value)
            : null;

        profile.UpdatePersonalInfo(fullName, dateOfBirth, request.Gender);

        await _userProfileRepository.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Personal info updated successfully for user {UserId}", userId);
        
        return await MapToResponseAsync(profile, cancellationToken);
    }

    public async Task<UserProfileResponse> UpdatePhysicalMeasurementsAsync(Guid userId, UpdatePhysicalMeasurementsRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating physical measurements for user {UserId}", userId);
        
        await _validationService.ValidateAsync(request);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var measurements = CreatePhysicalMeasurements(request.Height, request.Weight, request.Units);
        profile.UpdatePhysicalMeasurements(measurements);

        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Physical measurements updated for user {UserId} - Height: {Height}, Weight: {Weight}", 
            userId, request.Height, request.Weight);

        // Publish event for cross-module synchronization
        var measurementsEvent = new PhysicalMeasurementsUpdatedEvent(
            userId,
            request.Height,
            request.Units?.HeightUnit,
            request.Weight,
            request.Units?.WeightUnit,
            DateTime.UtcNow,
            "ProfileUpdate");

        await _mediator.Publish(measurementsEvent, cancellationToken);

        return await MapToResponseAsync(profile, cancellationToken);
    }

    public async Task<UserProfileResponse> UpdateFitnessProfileAsync(Guid userId, UpdateFitnessProfileRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating fitness profile for user {UserId}", userId);
        
        await _validationService.ValidateAsync(request);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        profile.UpdateFitnessProfile(request.FitnessLevel, request.FitnessGoal);

        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fitness profile updated for user {UserId} - Level: {FitnessLevel}, Goal: {FitnessGoal}", 
            userId, request.FitnessLevel, request.FitnessGoal);

        return await MapToResponseAsync(profile, cancellationToken);
    }

    public async Task<ProfileOperationResponse> DeleteUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user profile for user {UserId}", userId);
        
        var success = await _userProfileRepository.DeleteAsync(userId, cancellationToken);
        
        if (!success)
        {
            _logger.LogError("Failed to delete user profile for user {UserId}", userId);
            throw UserDomainException.FailedToDeleteUserProfile(userId);
        }

        _logger.LogInformation("User profile deleted successfully for user {UserId}", userId);

        return new ProfileOperationResponse("Profile deleted successfully");
    }

    public async Task<SubscriptionResponse?> GetUserSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return profile?.Subscription == null ? null : _mapper.Map<SubscriptionResponse>(profile.Subscription);
    }

    public async Task<SubscriptionResponse> UpdateSubscriptionAsync(Guid userId, UpdateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        await _validationService.ValidateAsync(request);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var subscription = new Subscription(profile, request.Level, request.StartDate, request.EndDate);
        profile.UpdateSubscription(subscription);

        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SubscriptionResponse>(subscription);
    }

    public async Task<ProfileOperationResponse> CancelSubscriptionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        if (profile.Subscription == null)
        {
            throw UserDomainException.NoActiveSubscriptionFound();
        }

        profile.Subscription.Cancel();
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        return new ProfileOperationResponse("Subscription cancelled successfully");
    }

    public async Task<SubscriptionResponse> RenewSubscriptionAsync(Guid userId, DateTime newEndDate, CancellationToken cancellationToken = default)
    {
        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        if (profile.Subscription == null)
        {
            throw UserDomainException.NoSubscriptionToRenew();
        }

        profile.Subscription.Renew(newEndDate);
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SubscriptionResponse>(profile.Subscription);
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
            throw UserDomainException.UserProfileNotFound(userId);
        }

        return profile;
    }

    private static FullName CreateFullName(string? firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            return FullName.Empty;

        return FullName.Create(firstName?.Trim(), lastName?.Trim());
    }

    private static PhysicalMeasurements CreatePhysicalMeasurements(decimal? height, decimal? weight)
    {
        if (height == null && weight == null)
            return PhysicalMeasurements.Empty;

        return PhysicalMeasurements.Create(height, weight);
    }

    private static PhysicalMeasurements CreatePhysicalMeasurements(decimal? height, decimal? weight, MeasurementUnits? units = null)
    {
        if (height == null && weight == null)
            return PhysicalMeasurements.Empty;

        // Enregistrer les valeurs exactes saisies par l'utilisateur avec leurs unités
        return PhysicalMeasurements.Create(
            height, 
            weight, 
            units?.HeightUnit, 
            units?.WeightUnit);
    }

    /// <summary>
    /// Helper method to map UserProfile to UserProfileResponse with user's preferred units
    /// </summary>
    private async Task<UserProfileResponse> MapToResponseAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        var preferredUnits = await _userPreferenceService.GetUserPreferredUnitsAsync(profile.UserId, cancellationToken);
        
        return _mapper.Map<UserProfileResponse>(profile, opts =>
        {
            opts.Items["HeightUnit"] = preferredUnits.heightUnit;
            opts.Items["WeightUnit"] = preferredUnits.weightUnit;
        });
    }
}
