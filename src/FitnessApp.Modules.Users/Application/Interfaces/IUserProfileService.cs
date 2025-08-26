using FitnessApp.SharedKernel.DTOs.UserProfile.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;

namespace FitnessApp.Modules.Users.Application.Interfaces;

/// <summary>
/// Interface for user profile service.
/// Handles only profile-related operations, not authentication.
/// </summary>
public interface IUserProfileService
{
    // Profile query operations
    Task<UserProfileDto?> GetByUserIdAsync(Guid userId);
    Task<UserProfileListDto?> GetListDtoByUserIdAsync(Guid userId);
    
    // Profile command operations
    Task<UserProfileDto> CreateProfileAsync(CreateUserProfileRequest request);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request);
    Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request);
    
    // Profile queries with pagination
    Task<PagedResult<UserProfileDto>> GetProfilesAsync(UserProfileQueryRequest request);
    Task<UserProfileStatsDto> GetProfileStatsAsync();
    
    // Validation operations
    Task<bool> ProfileExistsAsync(Guid userId);
    
    // Business operations
    Task<bool> HasCompletedProfileAsync(Guid userId);
    Task<bool> CanAccessPremiumFeaturesAsync(Guid userId);
}
