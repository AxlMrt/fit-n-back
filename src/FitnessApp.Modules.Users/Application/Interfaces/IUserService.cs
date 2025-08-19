using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Application.DTOs.Responses;

namespace FitnessApp.Modules.Users.Application.Interfaces;

public interface IUserService
{
    Task<UserResponse?> GetUserByIdAsync(Guid userId);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<UserResponse> UpdateUserProfileAsync(Guid userId, UpdateProfileRequest profileDto);
    Task<bool> DeleteUserAsync(Guid userId);
    Task<bool> IsServiceHealthyAsync();

    // New user-centric operations
    Task UpdatePreferencesAsync(Guid userId, PreferencesUpdateRequest request);
    Task<UserGoalsResponse> GetGoalsAsync(Guid userId);
    Task<UserStatsResponse> GetStatsAsync(Guid userId);
}