using System.ComponentModel.DataAnnotations;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Responses;

public sealed record UserProfileDto(
    string? FirstName,
    string? LastName,
    string DisplayName,
    DateTime? DateOfBirth,
    int? Age,
    string? AgeGroup,
    Gender? Gender,
    string? GenderDisplay,
    decimal? Height,
    decimal? Weight,
    decimal? BMI,
    string? BMICategory,
    FitnessLevel? FitnessLevel,
    FitnessGoal? PrimaryFitnessGoal,
    bool IsCompleted
);

public sealed record UserSecurityDto(
    Role Role,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    bool IsLockedOut,
    DateTime? LockoutEnd,
    int AccessFailedCount
);

public sealed record UserSubscriptionDto(
    Guid Id,
    SubscriptionLevel Level,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    int DaysRemaining
);

public sealed record UserPreferenceDto(
    string Category,
    string Key,
    string Value,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record UserListDto(
    Guid Id,
    string Email,
    string Username,
    string DisplayName,
    Gender? Gender,
    int? Age,
    FitnessLevel? FitnessLevel,
    Role Role,
    bool EmailConfirmed,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public sealed record UserStatsDto(
    int TotalUsers,
    int ActiveUsers,
    int VerifiedUsers,
    int PremiumUsers,
    Dictionary<string, int> UsersByRole,
    Dictionary<string, int> UsersByFitnessLevel,
    Dictionary<string, int> UsersByAgeGroup
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
