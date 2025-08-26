using FitnessApp.SharedKernel.Enums;
using FitnessApp.SharedKernel.DTOs.UserProfile.Responses;

namespace FitnessApp.Modules.Users.Application.Interfaces;

public interface ISubscriptionService
{
    /// <summary>
    /// Creates a new subscription for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="level">The subscription level.</param>
    /// <param name="startDate">The start date of the subscription.</param>
    /// <param name="endDate">The end date of the subscription.</param>
    /// <returns>The created subscription identifier.</returns>
    Task<Guid> CreateSubscriptionAsync(Guid userId, SubscriptionLevel level, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Updates an existing subscription.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="level">The new subscription level.</param>
    /// <param name="endDate">The new end date of the subscription.</param>
    /// <returns>True if the subscription was updated, false otherwise.</returns>
    Task<bool> UpdateSubscriptionAsync(Guid userId, SubscriptionLevel level, DateTime endDate);

    /// <summary>
    /// Cancels a user's subscription.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>True if the subscription was cancelled, false otherwise.</returns>
    Task<bool> CancelSubscriptionAsync(Guid userId);

    /// <summary>
    /// Gets the current subscription for a user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The subscription details.</returns>
    Task<SubscriptionDto?> GetCurrentSubscriptionAsync(Guid userId);
}
