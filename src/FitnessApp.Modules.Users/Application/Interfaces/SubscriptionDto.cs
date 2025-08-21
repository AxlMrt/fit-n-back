using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Application.Interfaces;

/// <summary>
/// Data transfer object for subscription information.
/// </summary>
public class SubscriptionDto
{
    /// <summary>
    /// Gets or sets the subscription identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user identifier.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the subscription level.
    /// </summary>
    public SubscriptionLevel Level { get; set; }

    /// <summary>
    /// Gets or sets the start date of the subscription.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date of the subscription.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Gets a value indicating whether the subscription is active.
    /// </summary>
    public bool IsActive => DateTime.UtcNow >= StartDate && DateTime.UtcNow <= EndDate;
}
