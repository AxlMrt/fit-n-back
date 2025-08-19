
namespace FitnessApp.Modules.Users.Application.DTOs.Responses;

public record UserResponse(
    Guid Id,
    string Email,
    string UserName,
    UserProfileResponse? Profile,
    SubscriptionResponse? Subscription,
    IEnumerable<PreferenceResponse> Preferences
);