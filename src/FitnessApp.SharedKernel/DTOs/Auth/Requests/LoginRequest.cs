namespace FitnessApp.SharedKernel.DTOs.Auth.Requests;

/// <summary>
/// Represents a request to log in a user.
/// </summary>
public record LoginRequest(
    string Email,
    string Password
);
