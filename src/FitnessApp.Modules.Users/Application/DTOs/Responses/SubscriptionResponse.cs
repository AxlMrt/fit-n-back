
namespace FitnessApp.Modules.Users.Application.DTOs.Responses;

public record SubscriptionResponse(
    Guid Id,
    string Plan,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);