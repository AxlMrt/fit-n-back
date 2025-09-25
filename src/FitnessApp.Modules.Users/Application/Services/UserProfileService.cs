using AutoMapper;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Domain.Entities;
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
    private readonly IValidationService _validationService;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public UserProfileService(
        IUserProfileRepository userProfileRepository,
        IValidationService validationService,
        IMapper mapper,
        IMediator mediator)
    {
        _userProfileRepository = userProfileRepository ?? throw new ArgumentNullException(nameof(userProfileRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<UserProfileResponse?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return profile == null ? null : _mapper.Map<UserProfileResponse>(profile);
    }

    public async Task<UserProfileSummaryResponse?> GetUserProfileSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        
        return profile == null ? null : _mapper.Map<UserProfileSummaryResponse>(profile);
    }

    public async Task<UserProfileResponse> CreateUserProfileAsync(Guid userId, CreateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        await _validationService.ValidateAsync(request);

        var existingProfile = await _userProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingProfile != null)
        {
            throw new InvalidOperationException("User profile already exists");
        }
        var profile = new UserProfile(userId);
        
        var fullName = CreateFullName(request.FirstName, request.LastName);
        var dateOfBirth = DateOfBirth.Create(request.DateOfBirth);
        profile.UpdatePersonalInfo(fullName, dateOfBirth, request.Gender);

        var measurements = CreatePhysicalMeasurements(request.HeightCm, request.WeightKg);
        profile.UpdatePhysicalMeasurements(measurements);

        profile.UpdateFitnessProfile(request.FitnessLevel, request.PrimaryFitnessGoal);

        await _userProfileRepository.AddAsync(profile, cancellationToken);
        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserProfileResponse>(profile);
    }

    public async Task<UserProfileResponse> UpdatePersonalInfoAsync(Guid userId, UpdatePersonalInfoRequest request, CancellationToken cancellationToken = default)
    {
        await _validationService.ValidateAsync(request);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var fullName = request.FirstName != null || request.LastName != null
            ? CreateFullName(request.FirstName, request.LastName)
            : null;
        
        var dateOfBirth = request.DateOfBirth.HasValue
            ? DateOfBirth.Create(request.DateOfBirth.Value)
            : null;

        profile.UpdatePersonalInfo(fullName, dateOfBirth, request.Gender);

        await _userProfileRepository.SaveChangesAsync(cancellationToken);
        
        return _mapper.Map<UserProfileResponse>(profile);
    }

    public async Task<UserProfileResponse> UpdatePhysicalMeasurementsAsync(Guid userId, UpdatePhysicalMeasurementsRequest request, CancellationToken cancellationToken = default)
    {
        await _validationService.ValidateAsync(request);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        var measurements = CreatePhysicalMeasurements(request.Height, request.Weight, request.Units);
        profile.UpdatePhysicalMeasurements(measurements);

        await _userProfileRepository.SaveChangesAsync(cancellationToken);

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

        return _mapper.Map<UserProfileResponse>(profile);
    }

    public async Task<UserProfileResponse> UpdateFitnessProfileAsync(Guid userId, UpdateFitnessProfileRequest request, CancellationToken cancellationToken = default)
    {
        await _validationService.ValidateAsync(request);

        var profile = await GetProfileOrThrowAsync(userId, cancellationToken);

        profile.UpdateFitnessProfile(request.FitnessLevel, request.PrimaryFitnessGoal);

        await _userProfileRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserProfileResponse>(profile);
    }

    public async Task<ProfileOperationResponse> DeleteUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var success = await _userProfileRepository.DeleteAsync(userId, cancellationToken);
        
        if (!success)
        {
            throw new InvalidOperationException($"Failed to delete user profile for user {userId}");
        }

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
            throw new InvalidOperationException("No active subscription found");
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
            throw new InvalidOperationException("No subscription found to renew");
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
            throw new InvalidOperationException($"User profile not found for user {userId}");
        }

        return profile;
    }

    private static FullName CreateFullName(string? firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            return FullName.Empty;

        return FullName.Create(firstName?.Trim(), lastName?.Trim());
    }

    private static PhysicalMeasurements CreatePhysicalMeasurements(int? heightCm, decimal? weightKg)
    {
        if (heightCm == null && weightKg == null)
            return PhysicalMeasurements.Empty;

        return PhysicalMeasurements.Create((decimal?)heightCm, weightKg);
    }

    private static PhysicalMeasurements CreatePhysicalMeasurements(decimal? height, decimal? weight, MeasurementUnits? units = null)
    {
        if (height == null && weight == null)
            return PhysicalMeasurements.Empty;

        decimal? heightCm = null;
        decimal? weightKg = null;

        if (height.HasValue)
        {
            var heightUnit = units?.HeightUnit ?? "cm";
            heightCm = FitnessApp.SharedKernel.Services.MeasurementUnitConverter.ConvertHeightToCentimeters(height.Value, heightUnit);
        }

        if (weight.HasValue)
        {
            var weightUnit = units?.WeightUnit ?? "kg";
            weightKg = FitnessApp.SharedKernel.Services.MeasurementUnitConverter.ConvertWeightToKilograms(weight.Value, weightUnit);
        }

        return PhysicalMeasurements.Create(heightCm, weightKg);
    }
}