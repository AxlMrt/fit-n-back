namespace FitnessApp.SharedKernel.DTOs.Auth.Requests;

/// <summary>
/// Represents a request to refresh an authentication token.
/// </summary>
public record RefreshTokenRequest(
    string RefreshToken
);
