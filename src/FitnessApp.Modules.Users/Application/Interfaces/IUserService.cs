using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Application.DTOs.Responses;
using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Application.Interfaces;

/// <summary>
/// Interface for user application services.
/// Defines the contract for user management operations.
/// </summary>
public interface IUserService
{
    // Query operations
    Task<UserDto?> GetByIdAsync(Guid userId);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetByUsernameAsync(string username);
    
    // Command operations
    Task<UserDto> CreateUserAsync(CreateUserRequest request);
    Task<UserDto> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request);
    Task UpdateUserPreferencesAsync(Guid userId, UpdatePreferencesRequest request);
    
    // Validation operations
    Task<bool> ExistsWithEmailAsync(string email);
    Task<bool> ExistsWithUsernameAsync(string username);
    
    // Admin operations
    Task<UserDto> UpdateUserRoleAsync(Guid userId, Role role);
    Task<bool> DeactivateUserAsync(Guid userId);
}