using FitnessApp.Modules.Authorization.Enums;
using FitnessApp.Modules.Users.Application.Exceptions;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Users.Application.Services;

/// <summary>
/// Service for managing user subscriptions
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(
        IUserRepository userRepository,
        ILogger<SubscriptionService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<Guid> CreateSubscriptionAsync(Guid userId, SubscriptionLevel level, DateTime startDate, DateTime endDate)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found when creating subscription", userId);
            throw new NotFoundException($"User with ID {userId} not found");
        }

        if (user.Subscription != null && user.Subscription.IsActive)
        {
            _logger.LogWarning("User {UserId} already has an active subscription", userId);
            throw new InvalidOperationException("User already has an active subscription");
        }

        if (startDate >= endDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }

        var subscription = new Subscription(user, level, startDate, endDate);
        user.UpdateSubscription(subscription);

        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Created subscription {SubscriptionId} for user {UserId}", subscription.Id, userId);
        return subscription.Id;
    }

    public async Task<bool> UpdateSubscriptionAsync(Guid userId, SubscriptionLevel level, DateTime endDate)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found when updating subscription", userId);
            throw new NotFoundException($"User with ID {userId} not found");
        }

        if (user.Subscription == null || !user.Subscription.IsActive)
        {
            _logger.LogWarning("User {UserId} does not have an active subscription to update", userId);
            throw new InvalidOperationException("User does not have an active subscription");
        }

        if (endDate <= DateTime.UtcNow)
        {
            throw new ArgumentException("End date must be in the future");
        }

        user.Subscription.UpdateSubscription(level, endDate);
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Updated subscription for user {UserId}", userId);
        return true;
    }

    public async Task<bool> CancelSubscriptionAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found when cancelling subscription", userId);
            throw new NotFoundException($"User with ID {userId} not found");
        }

        if (user.Subscription == null || !user.Subscription.IsActive)
        {
            _logger.LogWarning("User {UserId} does not have an active subscription to cancel", userId);
            return false;
        }

        user.Subscription.Cancel();
        await _userRepository.UpdateAsync(user);

        _logger.LogInformation("Cancelled subscription for user {UserId}", userId);
        return true;
    }

    public async Task<SubscriptionDto?> GetCurrentSubscriptionAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user?.Subscription == null)
        {
            return null;
        }

        var subscription = user.Subscription;
        return new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            Level = subscription.Level,
            StartDate = subscription.StartDate,
            EndDate = subscription.EndDate
        };
    }
}
