namespace FitnessApp.Modules.Authorization;

/// <summary>
/// Contains constants for custom claim types used in the application.
/// </summary>
public static class FitnessAppClaimTypes
{
    /// <summary>
    /// Claim type for the user's subscription level.
    /// </summary>
    public const string SubscriptionLevel = "SubscriptionLevel";
    
    /// <summary>
    /// Claim type for the user's subscription expiration date.
    /// </summary>
    public const string SubscriptionExpiration = "SubscriptionExpiration";
}
