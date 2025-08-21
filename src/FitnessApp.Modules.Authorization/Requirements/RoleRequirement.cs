using Microsoft.AspNetCore.Authorization;

namespace FitnessApp.Modules.Authorization.Requirements;

/// <summary>
/// Requirement that ensures the user has a specific role.
/// </summary>
public class RoleRequirement : IAuthorizationRequirement
{
    public string[] RequiredRoles { get; }

    public RoleRequirement(params string[] requiredRoles)
    {
        RequiredRoles = requiredRoles;
    }
}
