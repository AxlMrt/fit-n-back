using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Application.DTOs.Requests;

public sealed record UpdateUserRoleRequest(
    Role Role
);

public sealed record CreateSubscriptionRequest(
    SubscriptionLevel Level,
    DateTime StartDate,
    DateTime EndDate
);

public sealed record UpdateSubscriptionRequest(
    SubscriptionLevel Level,
    DateTime EndDate
);
