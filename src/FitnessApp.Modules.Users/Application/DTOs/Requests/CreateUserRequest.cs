namespace FitnessApp.Modules.Users.Application.DTOs.Requests;

public record CreateUserRequest(
    string Email,
    string UserName,
    string? FirstName,
    string? LastName,
    string Password
);