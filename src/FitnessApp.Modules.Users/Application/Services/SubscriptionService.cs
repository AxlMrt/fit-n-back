using FitnessApp.SharedKernel.Enums;
using FitnessApp.Modules.Users.Application.Exceptions;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Application.Mapping;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.SharedKernel.DTOs.UserProfile.Responses;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Users.Application.Services;

/// <summary>
/// Service for managing user subscriptions
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IUserProfileRepository _userProfileRepository;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        IUserProfileRepository userProfileRepository,
        ILogger<SubscriptionService> logger)
    {
        _userProfileRepository = userProfileRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateSubscriptionAsync(Guid userId, SubscriptionLevel level, DateTime startDate, DateTime endDate)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
        {
            throw new UserNotFoundException($"User with ID {userId} not found");
        }

        // Check if user already has an active subscription
        if (userProfile.Subscription != null)
        {
            throw new InvalidOperationException("User already has an active subscription");
        }

        var subscription = new Subscription(userProfile, level, startDate, endDate);
        userProfile.UpdateSubscription(subscription);

        await _userProfileRepository.UpdateAsync(userProfile);

        _logger.LogInformation("Created subscription {SubscriptionId} for user {UserId}", 
            subscription.Id, userId);

        return subscription.Id;
    }

    public async Task<bool> UpdateSubscriptionAsync(Guid userId, SubscriptionLevel level, DateTime endDate)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
        {
            return false;
        }

        var activeSubscription = userProfile.Subscription;
        if (activeSubscription == null)
        {
            return false;
        }

        // Create new subscription
        var newSubscription = new Subscription(userProfile, level, DateTime.UtcNow, endDate);
        userProfile.UpdateSubscription(newSubscription);

        await _userProfileRepository.UpdateAsync(userProfile);

        _logger.LogInformation("Updated subscription for user {UserId}", userId);
        return true;
    }

    public async Task<bool> CancelSubscriptionAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
        {
            return false;
        }

        var activeSubscription = userProfile.Subscription;
        if (activeSubscription == null)
        {
            return false;
        }

        // Remove subscription
        userProfile.UpdateSubscription(null);

        await _userProfileRepository.UpdateAsync(userProfile);

        _logger.LogInformation("Cancelled subscription for user {UserId}", userId);
        return true;
    }

    public async Task<SubscriptionDto?> GetCurrentSubscriptionAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        return userProfile?.Subscription?.MapToDto();
    }

    public async Task<bool> HasActiveSubscriptionAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        return userProfile?.Subscription?.IsActive ?? false;
    }

    public async Task<SubscriptionLevel?> GetCurrentSubscriptionLevelAsync(Guid userId)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        return userProfile?.Subscription?.Level;
    }

    public async Task ExtendSubscriptionAsync(Guid userId, DateTime newEndDate)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
        {
            throw new UserNotFoundException($"User with ID {userId} not found");
        }

        var activeSubscription = userProfile.Subscription;
        if (activeSubscription == null)
        {
            throw new InvalidOperationException("No active subscription to extend");
        }

        // Create new subscription that starts when current one ends
        var newSubscription = new Subscription(userProfile, activeSubscription.Level, 
            activeSubscription.EndDate.AddDays(1), newEndDate);
        userProfile.UpdateSubscription(newSubscription);

        await _userProfileRepository.UpdateAsync(userProfile);

        _logger.LogInformation("Extended subscription for user {UserId} until {EndDate}", 
            userId, newEndDate);
    }

    public async Task UpgradeSubscriptionAsync(Guid userId, SubscriptionLevel newLevel)
    {
        var userProfile = await _userProfileRepository.GetByUserIdAsync(userId);
        if (userProfile == null)
        {
            throw new UserNotFoundException($"User with ID {userId} not found");
        }

        var activeSubscription = userProfile.Subscription;
        if (activeSubscription == null)
        {
            throw new InvalidOperationException("No active subscription to upgrade");
        }

        // Create new subscription
        var newSubscription = new Subscription(userProfile, newLevel, DateTime.UtcNow, activeSubscription.EndDate);
        userProfile.UpdateSubscription(newSubscription);

        await _userProfileRepository.UpdateAsync(userProfile);

        _logger.LogInformation("Upgraded subscription for user {UserId} to {Level}", 
            userId, newLevel);
    }
}
