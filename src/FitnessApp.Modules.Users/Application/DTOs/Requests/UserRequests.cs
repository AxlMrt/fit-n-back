using FitnessApp.SharedKernel.Enums;
using FitnessApp.Modules.Users.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Modules.Users.Application.DTOs.Requests;

public sealed record CreateUserRequest(
    [Required]
    [EmailAddress]
    [StringLength(320, MinimumLength = 5)]
    string Email,

    [Required]
    [StringLength(30, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9._-]{3,30}$")]
    string Username,

    [Required]
    [StringLength(128, MinimumLength = 8)]
    string Password,

    [StringLength(50)]
    string? FirstName = null,

    [StringLength(50)]
    string? LastName = null
);

public sealed record UpdateUserProfileRequest(
    [StringLength(50)]
    string? FirstName,

    [StringLength(50)]
    string? LastName,

    DateTime? DateOfBirth,

    Gender? Gender,

    [Range(50, 250)]
    decimal? HeightCm,

    [Range(20, 300)]
    decimal? WeightKg,

    FitnessLevel? FitnessLevel,

    FitnessGoal? PrimaryFitnessGoal
);

public sealed record UpdateUserEmailRequest(
    [Required]
    [EmailAddress]
    [StringLength(320, MinimumLength = 5)]
    string NewEmail
);

public sealed record ChangePasswordRequest(
    [Required]
    string CurrentPassword,

    [Required]
    [StringLength(128, MinimumLength = 8)]
    string NewPassword
);

public sealed record UpdatePreferencesRequest(
    Dictionary<string, Dictionary<string, string>> Preferences
);

public sealed record UserQueryRequest(
    string? EmailFilter = null,
    string? NameFilter = null,
    Gender? GenderFilter = null,
    FitnessLevel? FitnessLevelFilter = null,
    bool? IsActiveFilter = null,
    string SortBy = "CreatedAt",
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20
);
