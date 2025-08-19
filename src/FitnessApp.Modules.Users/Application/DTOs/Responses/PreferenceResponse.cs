
namespace FitnessApp.Modules.Users.Application.DTOs.Responses;

public record PreferenceResponse(
    Guid Id,
    string Category,
    string Key,
    string Value
);