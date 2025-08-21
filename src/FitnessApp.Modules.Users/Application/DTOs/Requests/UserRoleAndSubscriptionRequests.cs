using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Application.DTOs.Requests;

/// <summary>
/// Request pour mettre à jour le rôle d'un utilisateur.
/// </summary>
public record UpdateUserRoleRequest(Role Role);

/// <summary>
/// Request pour créer un nouvel abonnement pour un utilisateur.
/// </summary>
public record CreateSubscriptionRequest(
    SubscriptionLevel Level,
    DateTime StartDate,
    DateTime EndDate
);

/// <summary>
/// Request pour mettre à jour un abonnement existant.
/// </summary>
public record UpdateSubscriptionRequest(
    SubscriptionLevel Level,
    DateTime EndDate
);
