using FitnessApp.Modules.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FitnessApp.Modules.Authorization.Handlers;

/// <summary>
/// Handler for validating active subscription requirements.
/// </summary>
public class ActiveSubscriptionHandler : AuthorizationHandler<ActiveSubscriptionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        ActiveSubscriptionRequirement requirement)
    {
        if (!context.User.HasClaim(c => c.Type == FitnessAppClaimTypes.SubscriptionLevel))
        {
            return Task.CompletedTask; 
        }

        var subscriptionLevel = context.User.FindFirst(c => c.Type == FitnessAppClaimTypes.SubscriptionLevel)?.Value;

        if (subscriptionLevel != null && requirement.RequiredLevels.Contains(subscriptionLevel))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
