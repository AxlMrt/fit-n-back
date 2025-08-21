
using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Application.DTOs.Responses;

public record UserResponse(
    Guid Id,
    string Email,
    string UserName,
    Role Role,
    UserProfileResponse? Profile,
    SubscriptionResponse? Subscription,
    IEnumerable<PreferenceResponse> Preferences
);