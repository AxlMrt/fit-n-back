namespace FitnessApp.SharedKernel.DTOs.Auth.Requests;

/// <summary>
/// Represents a request to reset a user's password.
/// </summary>
public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword
);
