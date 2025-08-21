using FitnessApp.Modules.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FitnessApp.Modules.Authorization.Handlers;

/// <summary>
/// Handler for validating role requirements.
/// </summary>
public class RoleHandler : AuthorizationHandler<RoleRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        RoleRequirement requirement)
    {
        // Check if the user has any of the required roles
        foreach (var role in requirement.RequiredRoles)
        {
            if (context.User.IsInRole(role))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}
