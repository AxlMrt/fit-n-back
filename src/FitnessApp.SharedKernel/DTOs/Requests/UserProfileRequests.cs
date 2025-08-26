using System.ComponentModel.DataAnnotations;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.UserProfile.Requests;

/// <summary>
/// Request to create a new user profile after authentication registration.
/// Contains only profile-related information, not authentication data.
/// </summary>
public record CreateUserProfileRequest(
    [Required]
    Guid UserId,

    [Required]
    [StringLength(100, MinimumLength = 1)]
    string FirstName,

    [Required]
    [StringLength(100, MinimumLength = 1)]
    string LastName,

    [DataType(DataType.Date)]
    DateTime? DateOfBirth = null,

    Gender? Gender = null,

    [Range(50, 250, ErrorMessage = "Height must be between 50cm and 250cm")]
    decimal? Height = null,

    [Range(20, 300, ErrorMessage = "Weight must be between 20kg and 300kg")]
    decimal? Weight = null,

    FitnessLevel? FitnessLevel = null,

    [StringLength(500)]
    string? Bio = null,

    SubscriptionLevel SubscriptionLevel = SubscriptionLevel.Free
);

/// <summary>
/// Request to update an existing user profile.
/// </summary>
public record UpdateUserProfileRequest(
    [StringLength(100, MinimumLength = 1)]
    string? FirstName = null,

    [StringLength(100, MinimumLength = 1)]
    string? LastName = null,

    [DataType(DataType.Date)]
    DateTime? DateOfBirth = null,

    Gender? Gender = null,

    [Range(50, 250, ErrorMessage = "Height must be between 50cm and 250cm")]
    decimal? Height = null,

    [Range(20, 300, ErrorMessage = "Weight must be between 20kg and 300kg")]
    decimal? Weight = null,

    FitnessLevel? FitnessLevel = null,

    [StringLength(500)]
    string? Bio = null
);

/// <summary>
/// Request to update user preferences.
/// </summary>
public record UpdatePreferencesRequest(
    bool? NotificationsEnabled = null,
    string? PreferredLanguage = null,
    string? TimeZone = null,
    bool? PublicProfile = null,
    bool? ShowAgePublicly = null,
    bool? ShowWeightProgressPublicly = null
);

/// <summary>
/// Request for querying user profiles with filters and pagination.
/// </summary>
public record UserProfileQueryRequest(
    string? SearchTerm = null,
    FitnessLevel? FitnessLevel = null,
    Gender? Gender = null,
    int? MinAge = null,
    int? MaxAge = null,
    SubscriptionLevel? SubscriptionLevel = null,
    bool? PublicProfilesOnly = true,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = "CreatedAt",
    bool Descending = true
);
