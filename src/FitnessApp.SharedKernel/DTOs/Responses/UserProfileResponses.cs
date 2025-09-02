using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Users.Responses;

/// <summary>
/// Response containing complete user profile information.
/// </summary>
public sealed record UserProfileResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    DateTime? DateOfBirth,
    int? Age,
    Gender? Gender,
    int? HeightCm,
    decimal? WeightKg,
    decimal? BMI,
    FitnessLevel? FitnessLevel,
    FitnessGoal? PrimaryFitnessGoal,
    SubscriptionResponse? Subscription,
    bool HasCompletedProfile,
    bool CanAccessPremiumFeatures,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Response containing basic user profile information.
/// </summary>
public sealed record UserProfileSummaryResponse(
    Guid UserId,
    string FullName,
    int? Age,
    Gender? Gender,
    FitnessLevel? FitnessLevel,
    SubscriptionLevel? SubscriptionLevel,
    bool HasCompletedProfile
);

/// <summary>
/// Response containing subscription information.
/// </summary>
public sealed record SubscriptionResponse(
    Guid Id,
    SubscriptionLevel Level,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);

/// <summary>
/// Response containing user preference information.
/// </summary>
public sealed record PreferenceResponse(
    Guid Id,
    string Category,
    string Key,
    string? Value,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Response containing grouped user preferences.
/// </summary>
public sealed record UserPreferencesResponse(
    Guid UserId,
    IDictionary<string, IDictionary<string, string?>> Preferences
);

/// <summary>
/// Response for successful profile operations.
/// </summary>
public sealed record ProfileOperationResponse(
    string Message,
    bool Success = true
);