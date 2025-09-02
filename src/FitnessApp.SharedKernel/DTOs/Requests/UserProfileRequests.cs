using System.ComponentModel.DataAnnotations;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Users.Requests;

/// <summary>
/// Request to create a new user profile after authentication.
/// </summary>
public sealed record CreateUserProfileRequest(
    [Required]
    [StringLength(50, MinimumLength = 2)]
    string FirstName,
    
    [Required]
    [StringLength(50, MinimumLength = 2)]
    string LastName,
    
    [Required]
    DateTime DateOfBirth,
    
    [Required]
    Gender Gender,
    
    [Required]
    [Range(1, 250)]
    int HeightCm,
    
    [Required]
    [Range(1, 500)]
    decimal WeightKg,
    
    [Required]
    FitnessLevel FitnessLevel,
    
    [Required]
    FitnessGoal PrimaryFitnessGoal
);

/// <summary>
/// Request to update personal information in user profile.
/// </summary>
public sealed record UpdatePersonalInfoRequest(
    [StringLength(50, MinimumLength = 2)]
    string? FirstName,
    
    [StringLength(50, MinimumLength = 2)]
    string? LastName,
    
    DateTime? DateOfBirth,
    
    Gender? Gender
);

/// <summary>
/// Request to update physical measurements.
/// </summary>
public sealed record UpdatePhysicalMeasurementsRequest(
    [Range(1, 250)]
    int? HeightCm,
    
    [Range(1, 500)]
    decimal? WeightKg
);

/// <summary>
/// Request to update fitness profile information.
/// </summary>
public sealed record UpdateFitnessProfileRequest(
    FitnessLevel? FitnessLevel,
    FitnessGoal? PrimaryFitnessGoal
);

/// <summary>
/// Request to create or update a user preference.
/// </summary>
public sealed record CreateOrUpdatePreferenceRequest(
    [Required]
    PreferenceCategory Category,
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    string Key,
    
    [StringLength(1000)]
    string? Value
);

/// <summary>
/// Request to update multiple preferences at once.
/// </summary>
public sealed record UpdatePreferencesRequest(
    [Required]
    IDictionary<PreferenceCategory, IDictionary<string, string?>> Preferences
);

/// <summary>
/// Request to update user subscription.
/// </summary>
public sealed record UpdateSubscriptionRequest(
    [Required]
    SubscriptionLevel Level,
    
    [Required]
    DateTime StartDate,
    
    [Required]
    DateTime EndDate
);