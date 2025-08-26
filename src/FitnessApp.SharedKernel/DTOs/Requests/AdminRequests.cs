using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Requests;

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
