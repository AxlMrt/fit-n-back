namespace FitnessApp.SharedKernel.DTOs.Auth.Requests;

/// <summary>
/// Represents a request to initiate the forgot password process.
/// </summary>
public record ForgotPasswordRequest(
    string Email
);
