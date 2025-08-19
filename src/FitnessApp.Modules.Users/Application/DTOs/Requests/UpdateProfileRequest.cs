namespace FitnessApp.Modules.Users.Application.DTOs.Requests;

public record UpdateProfileRequest(
    string? FirstName,
    string? LastName,
    DateTime? DateOfBirth,
    string? Gender,
    float? Height,
    float? Weight,
    string? FitnessLevel,
    string? FitnessGoal
);