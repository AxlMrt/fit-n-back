using FitnessApp.Modules.Authorization.Enums;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Repositories;

namespace FitnessApp.Modules.Users.Application.Services;

/// <summary>
/// Service for managing user subscriptions.
/// </summary>
public class SubscriptionService : ISubscriptionService
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="SubscriptionService"/> class.
    /// </summary>
    /// <param name="userRepository">The user repository.</param>
    public SubscriptionService(IUserRepository userRepository)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    /// <inheritdoc/>
    public async Task<Guid> CreateSubscriptionAsync(Guid userId, SubscriptionLevel level, DateTime startDate, DateTime endDate)
    {
        var user = await _userRepository.GetByIdAsync(userId) 
            ?? throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));

        var subscription = new Subscription(user, level, startDate, endDate);
        user.UpdateSubscription(subscription);

        await _userRepository.SaveChangesAsync();
        
        return subscription.Id;
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateSubscriptionAsync(Guid userId, SubscriptionLevel level, DateTime endDate)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));

        if (user.Subscription == null)
        {
            throw new InvalidOperationException($"User with ID {userId} doesn't have an active subscription.");
        }

        user.Subscription.UpdateSubscription(level, endDate);
        await _userRepository.SaveChangesAsync();
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> CancelSubscriptionAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));

        if (user.Subscription == null)
        {
            return false;
        }

        user.Subscription.Cancel();
        await _userRepository.SaveChangesAsync();
        
        return true;
    }

    /// <inheritdoc/>
    public async Task<SubscriptionDto?> GetCurrentSubscriptionAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user?.Subscription == null)
        {
            return null;
        }

        return new SubscriptionDto
        {
            Id = user.Subscription.Id,
            UserId = user.Id,
            Level = user.Subscription.Level,
            StartDate = user.Subscription.StartDate,
            EndDate = user.Subscription.EndDate
        };
    }
}
