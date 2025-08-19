
namespace FitnessApp.Modules.Users.Application.DTOs.Responses;

public record UserProfileResponse(
    Guid Id,
    string? FirstName,
    string? LastName,
    DateTime? DateOfBirth,
    string? Gender,
    float? Height,
    float? Weight,
    string? FitnessLevel,
    string? FitnessGoal,
    int? Age,
    float? BMI,
    string FullName
);
   