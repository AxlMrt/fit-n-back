using FitnessApp.Modules.Authorization.Handlers;
using FitnessApp.Modules.Authorization.Policies;
using FitnessApp.Modules.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Authorization;

public static class AuthorizationModuleExtensions
{
    public static IServiceCollection AddAuthorizationModule(this IServiceCollection services)
    {
        // Register authorization handlers
        services.AddSingleton<IAuthorizationHandler, RoleHandler>();
        services.AddSingleton<IAuthorizationHandler, ActiveSubscriptionHandler>();
        
        // Configure authorization policies
        services.AddAuthorizationCore(options =>
        {
            // Role-based policies
            options.AddPolicy(AuthorizationPolicies.RequireAdmin, policy =>
                policy.AddRequirements(new RoleRequirement(AuthorizationPolicies.GetAdminRoles())));
                
            options.AddPolicy(AuthorizationPolicies.RequireCoach, policy =>
                policy.AddRequirements(new RoleRequirement(AuthorizationPolicies.GetCoachRoles())));
                
            options.AddPolicy(AuthorizationPolicies.RequireAthleteOrCoach, policy =>
                policy.AddRequirements(new RoleRequirement(Role.Athlete.ToString(), Role.Coach.ToString(), Role.Admin.ToString())));
                
            // Subscription-based policies
            options.AddPolicy(AuthorizationPolicies.RequirePremiumSubscription, policy =>
                policy.AddRequirements(new ActiveSubscriptionRequirement(AuthorizationPolicies.GetPremiumLevels())));
                
            options.AddPolicy(AuthorizationPolicies.RequireEliteSubscription, policy =>
                policy.AddRequirements(new ActiveSubscriptionRequirement(SubscriptionLevel.Elite.ToString())));
                
            options.AddPolicy(AuthorizationPolicies.RequireAnyPaidSubscription, policy =>
                policy.AddRequirements(new ActiveSubscriptionRequirement(AuthorizationPolicies.GetPaidLevels())));
                
            // Combined policies
            options.AddPolicy(AuthorizationPolicies.RequireCoachWithPremium, policy =>
                policy.AddRequirements(
                    new RoleRequirement(AuthorizationPolicies.GetCoachRoles()),
                    new ActiveSubscriptionRequirement(AuthorizationPolicies.GetPremiumLevels())));
                
            // Feature-specific policies
            options.AddPolicy(AuthorizationPolicies.CanAccessAdvancedAnalytics, policy =>
                policy.AddRequirements(new ActiveSubscriptionRequirement(AuthorizationPolicies.GetPremiumLevels())));
                
            options.AddPolicy(AuthorizationPolicies.CanAccessExclusiveWorkouts, policy =>
                policy.AddRequirements(new ActiveSubscriptionRequirement(AuthorizationPolicies.GetPremiumLevels())));
                
            // User management policies
            options.AddPolicy(AuthorizationPolicies.CanManageUsers, policy =>
                policy.AddRequirements(new RoleRequirement(Role.Admin.ToString())));
                
            options.AddPolicy(AuthorizationPolicies.CanManageContent, policy =>
                policy.AddRequirements(new RoleRequirement(Role.Admin.ToString(), Role.Coach.ToString())));
        });

        return services;
    }
}
