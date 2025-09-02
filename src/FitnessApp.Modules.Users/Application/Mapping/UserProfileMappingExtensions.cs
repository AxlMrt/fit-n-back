using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.SharedKernel.DTOs.Users.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Mapping;

public static class UserProfileMappingExtensions
{
    public static UserProfileResponse ToResponse(this UserProfile profile)
    {
        return new UserProfileResponse(
            profile.UserId,
            profile.Name.FirstName ?? string.Empty,
            profile.Name.LastName ?? string.Empty,
            profile.DateOfBirth?.Value,
            profile.GetAge(),
            profile.Gender,
            (int?)profile.PhysicalMeasurements.HeightCm,
            profile.PhysicalMeasurements.WeightKg,
            profile.GetBMI(),
            profile.FitnessLevel,
            profile.PrimaryFitnessGoal,
            profile.Subscription?.ToResponse(),
            profile.HasCompletedProfile(),
            profile.CanAccessPremiumFeatures(),
            profile.CreatedAt,
            profile.UpdatedAt
        );
    }

    public static UserProfileSummaryResponse ToSummaryResponse(this UserProfile profile)
    {
        var fullName = $"{profile.Name.FirstName} {profile.Name.LastName}".Trim();
        if (string.IsNullOrEmpty(fullName))
            fullName = "User";

        return new UserProfileSummaryResponse(
            profile.UserId,
            fullName,
            profile.GetAge(),
            profile.Gender,
            profile.FitnessLevel,
            profile.Subscription?.Level,
            profile.HasCompletedProfile()
        );
    }

    public static SubscriptionResponse ToResponse(this Subscription subscription)
    {
        return new SubscriptionResponse(
            subscription.Id,
            subscription.Level,
            subscription.StartDate,
            subscription.EndDate,
            subscription.IsActive
        );
    }

    public static PreferenceResponse ToResponse(this Preference preference)
    {
        return new PreferenceResponse(
            preference.Id,
            preference.Category,
            preference.Key,
            preference.Value,
            preference.CreatedAt,
            preference.UpdatedAt
        );
    }

    public static UserPreferencesResponse ToPreferencesResponse(this UserProfile profile)
    {
        var groupedPreferences = profile.Preferences
            .GroupBy(p => p.Category)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(p => p.Key, p => (string?)p.Value) as IDictionary<string, string?>
            );

        return new UserPreferencesResponse(
            profile.UserId,
            groupedPreferences
        );
    }

    public static UserPreferencesResponse ToPreferencesByCategoryResponse(this UserProfile profile, PreferenceCategory category)
    {
        var categoryPreferences = profile.Preferences
            .Where(p => p.Category == category)
            .ToDictionary(p => p.Key, p => (string?)p.Value);

        var result = new Dictionary<PreferenceCategory, IDictionary<string, string?>>
        {
            { category, categoryPreferences }
        } as IDictionary<PreferenceCategory, IDictionary<string, string?>>;

        return new UserPreferencesResponse(
            profile.UserId,
            result
        );
    }

    public static FullName ToFullName(string? firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
            return FullName.Empty;

        return FullName.Create(
            firstName?.Trim(),
            lastName?.Trim()
        );
    }

    public static PhysicalMeasurements ToPhysicalMeasurements(int? heightCm, decimal? weightKg)
    {
        if (heightCm == null && weightKg == null)
            return PhysicalMeasurements.Empty;

        return PhysicalMeasurements.Create((decimal?)heightCm, weightKg);
    }
}