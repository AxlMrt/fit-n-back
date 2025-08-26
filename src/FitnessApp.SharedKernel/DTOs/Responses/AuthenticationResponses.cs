using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Auth.Responses;

/// <summary>
/// Represents the response for authentication operations.
/// </summary>
public record AuthResponse(
    Guid UserId,
    string UserName,
    string Email,
    string AccessToken,
    DateTime ExpiresAt,
    Role Role,
    SubscriptionLevel? SubscriptionLevel = null,
    string? RefreshToken = null,
    DateTime? RefreshTokenExpiresAt = null
);

/// <summary>
/// Authentication user DTO containing only auth-related information.
/// </summary>
public sealed record AuthUserDto(
    Guid Id,
    string Email,
    string Username,
    Role Role,
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

/// <summary>
/// Security status DTO for current user's security information.
/// </summary>
public sealed record SecurityStatusDto(
    bool EmailConfirmed,
    bool TwoFactorEnabled,
    bool IsLockedOut,
    DateTime? LockoutEnd,
    int AccessFailedCount,
    DateTime? LastLoginAt,
    bool HasPasswordResetToken,
    DateTime? SecurityStampUpdatedAt
);

public record EmailConfirmationResponse(
    string Message,
    bool EmailSent,
    DateTime? ExpiresAt = null
);

/// <summary>
/// Response for password reset initiation.
/// </summary>
public record PasswordResetResponse(
    string Message,
    bool EmailSent
);

/// <summary>
/// Response for successful password operations.
/// </summary>
public record PasswordChangeResponse(
    string Message,
    bool RequiresReauthentication = false
);
