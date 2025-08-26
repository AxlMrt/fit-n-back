using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.UserProfile.Responses;

/// <summary>
/// Full user profile DTO containing all profile information.
/// </summary>
public sealed record UserProfileDto(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string DisplayName,
    DateTime? DateOfBirth,
    int? Age,
    Gender? Gender,
    decimal? HeightInMeters,
    decimal? WeightInKilograms,
    FitnessLevel? FitnessLevel,
    string? Bio,
    SubscriptionLevel SubscriptionLevel,
    bool NotificationsEnabled,
    string? PreferredLanguage,
    string? TimeZone,
    bool PublicProfile,
    bool ShowAgePublicly,
    bool ShowWeightProgressPublicly,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    bool IsActive
);

/// <summary>
/// Lightweight user profile DTO for lists and search results.
/// </summary>
public sealed record UserProfileListDto(
    Guid Id,
    Guid UserId,
    string DisplayName,
    string FirstName,
    string LastName,
    int? Age,
    Gender? Gender,
    FitnessLevel? FitnessLevel,
    SubscriptionLevel SubscriptionLevel,
    string? Bio,
    DateTime CreatedAt
);

/// <summary>
/// Public profile DTO for other users viewing the profile.
/// Contains only publicly visible information.
/// </summary>
public sealed record PublicUserProfileDto(
    Guid Id,
    string DisplayName,
    int? Age,
    Gender? Gender,
    FitnessLevel? FitnessLevel,
    SubscriptionLevel SubscriptionLevel,
    string? Bio,
    DateTime CreatedAt
);

/// <summary>
/// User profile statistics DTO for admin/analytics.
/// </summary>
public sealed record UserProfileStatsDto(
    int TotalProfiles,
    int ActiveProfiles,
    int FreeUsers,
    int PremiumUsers,
    int CoachUsers,
    Dictionary<FitnessLevel, int> FitnessLevelDistribution,
    Dictionary<Gender, int> GenderDistribution,
    Dictionary<SubscriptionLevel, int> SubscriptionDistribution
);

/// <summary>
/// Profile creation response.
/// </summary>
public record ProfileCreationResponse(
    Guid ProfileId,
    string Message,
    bool Success = true
);

/// <summary>
/// Profile update response.
/// </summary>
public record ProfileUpdateResponse(
    string Message,
    bool Success = true,
    DateTime? UpdatedAt = null
);

/// <summary>
/// User preference DTO
/// </summary>
public sealed record PreferenceDto(
    Guid Id,
    string Category,
    string Key,
    string Value,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Subscription DTO
/// </summary>
public sealed record SubscriptionDto(
    Guid Id,
    SubscriptionLevel Level,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);
