using Microsoft.AspNetCore.Authorization;

namespace FitnessApp.Modules.Authorization.Requirements;

/// <summary>
/// Requirement that ensures the user has an active subscription of a minimum required level.
/// </summary>
public class ActiveSubscriptionRequirement : IAuthorizationRequirement
{
    public string[] RequiredLevels { get; }

    public ActiveSubscriptionRequirement(params string[] requiredLevels)
    {
        RequiredLevels = requiredLevels;
    }
}
