using FitnessApp.SharedKernel.Enums;
namespace FitnessApp.Modules.Authorization.Policies;

/// <summary>
/// Static class that defines all authorization policies in the application.
/// </summary>
public static class AuthorizationPolicies
{
    // Role-based policies
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireCoach = "RequireCoach";
    public const string RequireAthleteOrCoach = "RequireAthleteOrCoach";

    // Subscription-based policies
    public const string RequirePremiumSubscription = "RequirePremiumSubscription";
    public const string RequireEliteSubscription = "RequireEliteSubscription";
    public const string RequireAnyPaidSubscription = "RequireAnyPaidSubscription";

    // Combined policies
    public const string RequireCoachWithPremium = "RequireCoachWithPremium";

    // Premium content features policies
    public const string CanAccessAdvancedAnalytics = "CanAccessAdvancedAnalytics";
    public const string CanAccessExclusiveWorkouts = "CanAccessExclusiveWorkouts";

    // User management policies
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageContent = "CanManageContent";

    // Helper methods to get arrays of values for policies
    public static string[] GetPremiumLevels() => new[] {
        SubscriptionLevel.Premium.ToString(),
        SubscriptionLevel.Elite.ToString()
    };

    public static string[] GetPaidLevels() => new[] {
        SubscriptionLevel.Basic.ToString(),
        SubscriptionLevel.Premium.ToString(),
        SubscriptionLevel.Elite.ToString()
    };

    public static string[] GetAdminRoles() => new[] {
        Role.Admin.ToString()
    };

    public static string[] GetCoachRoles() => new[] {
        Role.Coach.ToString(),
        Role.Admin.ToString()
    };
}
