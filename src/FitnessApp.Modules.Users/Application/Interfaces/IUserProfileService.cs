using FitnessApp.SharedKernel.DTOs.UserProfile.Requests;
using UserProfileResponses = FitnessApp.SharedKernel.DTOs.UserProfile.Responses;
using FitnessApp.SharedKernel.DTOs.Responses;

namespace FitnessApp.Modules.Users.Application.Interfaces;

/// <summary>
/// Interface for user profile service.
/// Handles only profile-related operations, not authentication.
/// </summary>
public interface IUserProfileService
{
    // Profile query operations
    Task<UserProfileResponses.UserProfileDto?> GetByUserIdAsync(Guid userId);
    Task<UserProfileResponses.UserProfileListDto?> GetListDtoByUserIdAsync(Guid userId);
    
    // Profile command operations
    Task<UserProfileResponses.UserProfileDto> CreateProfileAsync(CreateUserProfileRequest request);
    Task<UserProfileResponses.UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request);
    Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request);
    
    // Profile queries with pagination
    Task<PagedResult<UserProfileResponses.UserProfileDto>> GetProfilesAsync(UserProfileQueryRequest request);
    Task<UserProfileResponses.UserProfileStatsDto> GetProfileStatsAsync();
    
    // Validation operations
    Task<bool> ProfileExistsAsync(Guid userId);
    
    // Business operations
    Task<bool> HasCompletedProfileAsync(Guid userId);
    Task<bool> CanAccessPremiumFeaturesAsync(Guid userId);
}
