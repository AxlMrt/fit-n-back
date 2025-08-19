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
    string? RefreshToken = null,
    DateTime? RefreshTokenExpiresAt = null
);
