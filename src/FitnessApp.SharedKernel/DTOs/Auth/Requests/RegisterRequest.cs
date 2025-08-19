namespace FitnessApp.SharedKernel.DTOs.Auth.Requests;

/// <summary>
/// Represents a request to register a new user.
/// </summary>
public record RegisterRequest(
    string Email,
    string UserName,
    string Password,
    string? FirstName,
    string? LastName
);
