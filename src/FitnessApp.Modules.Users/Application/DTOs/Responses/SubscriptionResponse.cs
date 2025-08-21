using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Application.DTOs.Responses;

public record SubscriptionResponse(
    Guid Id,
    SubscriptionLevel Level,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);
