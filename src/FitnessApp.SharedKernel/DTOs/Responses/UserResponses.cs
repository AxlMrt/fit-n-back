using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Responses;

public sealed record UserDto(
    Guid Id,
    string Email,
    string Username,
    string DisplayName,
    UserProfileDto Profile,
    UserSecurityDto Security,
    UserSubscriptionDto? Subscription,
    List<UserPreferenceDto> Preferences,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? LastLoginAt,
    bool IsActive
);

public sealed record UserProfileDto(
    string? FirstName,
    string? LastName,
    string DisplayName,
    DateTime? DateOfBirth,
    int? Age,
    string? AgeGroup,
    Gender? Gender,
    string? GenderDisplay,
    decimal? HeightCm,
    decimal? WeightKg,
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

public sealed record PagedUserResult(
    IEnumerable<UserListDto> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
