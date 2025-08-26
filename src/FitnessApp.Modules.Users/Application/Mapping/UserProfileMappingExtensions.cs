using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.SharedKernel.DTOs.UserProfile.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Mapping;

/// <summary>
/// Extension methods for mapping between UserProfile domain entities and DTOs.
/// </summary>
public static class UserProfileMappingExtensions
{
    public static UserProfileDto MapToDto(this UserProfile userProfile)
    {
        return new UserProfileDto(
            userProfile.UserId,
            userProfile.UserId,
            userProfile.Name?.FirstName ?? "",
            userProfile.Name?.LastName ?? "",
            userProfile.Name?.DisplayName ?? "",
            userProfile.DateOfBirth?.Value,
            userProfile.GetAge(),
            userProfile.Gender,
            userProfile.PhysicalMeasurements?.HeightCm / 100m, // Convert cm to meters
            userProfile.PhysicalMeasurements?.WeightKg,
            userProfile.FitnessLevel,
            null, // Bio - not available in current entity
            userProfile.Subscription?.Level ?? SubscriptionLevel.Free,
            userProfile.HasCompletedProfile(),
            null, // Phone - not available in current entity
            null, // Location - not available in current entity
            false, // ShowFitnessProgressPublicly - default value
            false, // ShowWeightProgressPublicly - default value
            userProfile.CanAccessPremiumFeatures(),
            userProfile.CreatedAt,
            userProfile.UpdatedAt,
            userProfile.Subscription?.IsActive ?? false
        );
    }

    public static UserProfileListDto MapToListDto(this UserProfile userProfile)
    {
        return new UserProfileListDto(
            userProfile.UserId,
            userProfile.UserId,
            userProfile.Name?.FirstName ?? "",
            userProfile.Name?.LastName ?? "",
            userProfile.Name?.DisplayName ?? "Anonymous",
            userProfile.GetAge(),
            userProfile.Gender,
            userProfile.FitnessLevel,
            userProfile.Subscription?.Level ?? SubscriptionLevel.Free,
            null, // Location - not available in current entity
            userProfile.CreatedAt
        );
    }

    public static PreferenceDto MapToDto(this Preference preference)
    {
        return new PreferenceDto(
            preference.Id,
            preference.Category,
            preference.Key,
            preference.Value,
            preference.CreatedAt,
            preference.UpdatedAt
        );
    }

    public static SubscriptionDto MapToDto(this Subscription subscription)
    {
        return new SubscriptionDto(
            subscription.Id,
            subscription.Level,
            subscription.StartDate,
            subscription.EndDate,
            subscription.IsActive
        );
    }
}
