using FitnessApp.Modules.Authorization.Enums;
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

    // User role operations
    Task<UserResponse> UpdateUserRoleAsync(Guid userId, Role role);

    // User preferences and stats
    Task UpdatePreferencesAsync(Guid userId, PreferencesUpdateRequest request);
    Task<UserGoalsResponse> GetGoalsAsync(Guid userId);
    Task<UserStatsResponse> GetStatsAsync(Guid userId);
}