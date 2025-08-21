
using FitnessApp.Modules.Authorization.Enums;
using FitnessApp.Modules.Users.Application.DTOs.Responses;
using FitnessApp.Modules.Users.Domain.Entities;

namespace FitnessApp.Modules.Users.Application.Mappers;

public static class UserMapper
{
    public static UserResponse MapToUserDto(User user)
    {
        return new UserResponse(
            user.Id,
            user.Email,
            user.UserName,
            user.Role,
            MapToUserProfileDto(user.Profile),
            user.Subscription != null ? MapToSubscriptionDto(user.Subscription) : null,
            user.Preferences.Select(MapToPreferenceDto)
        );
    }

    public static UserProfileResponse MapToUserProfileDto(UserProfile profile)
    {
        return new UserProfileResponse(
            profile.UserId,
            profile.FirstName,
            profile.LastName,
            profile.DateOfBirth,
            profile.Gender,
            profile.Height,
            profile.Weight,
            profile.FitnessLevel,
            profile.FitnessGoal,
            profile.CalculateAge(),
            profile.CalculateBMI(),
            profile.GetFullName()
        );
    }

    public static SubscriptionResponse MapToSubscriptionDto(Subscription subscription)
    {
        return new SubscriptionResponse(
            subscription.Id,
            subscription.Level,
            subscription.StartDate,
            subscription.EndDate,
            subscription.IsActive
        );
    }

    public static PreferenceResponse MapToPreferenceDto(Preference preference)
    {
        return new PreferenceResponse(
            preference.Id,
            preference.Category,
            preference.Key,
            preference.Value
        );
    }
}
           