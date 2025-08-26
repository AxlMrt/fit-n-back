using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.SharedKernel.DTOs.UserProfile.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessApp.API.Controllers.v1;

/// <summary>
/// User profile controller handling only profile-related operations.
/// Authentication operations are handled by the AuthController.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/profiles")]
[Produces("application/json")]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(
        IUserProfileService userProfileService, 
        ILogger<UserProfileController> logger)
    {
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    /// <returns>Current user's profile data</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var profile = await _userProfileService.GetByUserIdAsync(userId);
            if (profile == null)
            {
                return NotFound(new { message = "Profile not found" });
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { message = "An error occurred retrieving profile" });
        }
    }

    /// <summary>
    /// Create a new user profile
    /// </summary>
    /// <param name="request">Profile creation data</param>
    /// <returns>Created profile data</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 409)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateUserProfileRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            // Check if profile already exists
            if (await _userProfileService.ProfileExistsAsync(userId))
            {
                return Conflict(new { message = "Profile already exists" });
            }

            // Override the UserId from the request with the authenticated user's ID
            var profileRequest = request with { UserId = userId };
            
            var profile = await _userProfileService.CreateProfileAsync(profileRequest);
            _logger.LogInformation("Created profile for user {UserId}", userId);
            return CreatedAtAction(nameof(GetMyProfile), new { }, profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user profile");
            return StatusCode(500, new { message = "An error occurred creating profile" });
        }
    }

    /// <summary>
    /// Update current user's profile
    /// </summary>
    /// <param name="request">Profile update data</param>
    /// <returns>Updated profile data</returns>
    [HttpPut]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var profile = await _userProfileService.UpdateProfileAsync(userId, request);
            _logger.LogInformation("Updated profile for user {UserId}", userId);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "An error occurred updating profile" });
        }
    }

    /// <summary>
    /// Update current user's preferences
    /// </summary>
    /// <param name="request">Preferences data</param>
    /// <returns>Success message</returns>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            await _userProfileService.UpdatePreferencesAsync(userId, request);
            _logger.LogInformation("Updated preferences for user {UserId}", userId);
            return Ok(new { message = "Preferences updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preferences");
            return StatusCode(500, new { message = "An error occurred updating preferences" });
        }
    }

    /// <summary>
    /// Get profiles with search and filtering (public profiles only)
    /// </summary>
    /// <param name="request">Query parameters for filtering and pagination</param>
    /// <returns>Paginated list of profiles</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetProfiles([FromQuery] UserProfileQueryRequest request)
    {
        try
        {
            var profiles = await _userProfileService.GetProfilesAsync(request);
            return Ok(profiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profiles");
            return StatusCode(500, new { message = "An error occurred retrieving profiles" });
        }
    }

    /// <summary>
    /// Get profile by user ID (public profiles only)
    /// </summary>
    /// <param name="userId">User ID to get profile for</param>
    /// <returns>User profile data</returns>
    [HttpGet("{userId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetProfileByUserId(Guid userId)
    {
        try
        {
            var profile = await _userProfileService.GetListDtoByUserIdAsync(userId);
            if (profile == null)
            {
                return NotFound(new { message = "Profile not found" });
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile by user ID");
            return StatusCode(500, new { message = "An error occurred retrieving profile" });
        }
    }

    /// <summary>
    /// Check if current user has completed their profile
    /// </summary>
    /// <returns>Profile completion status</returns>
    [HttpGet("completion-status")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetCompletionStatus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var hasCompleted = await _userProfileService.HasCompletedProfileAsync(userId);
            var canAccessPremium = await _userProfileService.CanAccessPremiumFeaturesAsync(userId);
            
            return Ok(new 
            { 
                hasCompletedProfile = hasCompleted,
                canAccessPremiumFeatures = canAccessPremium
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking profile completion status");
            return StatusCode(500, new { message = "An error occurred checking profile status" });
        }
    }

    /// <summary>
    /// Get profile statistics (admin only)
    /// </summary>
    /// <returns>Profile statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetProfileStatistics()
    {
        try
        {
            var stats = await _userProfileService.GetProfileStatsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile statistics");
            return StatusCode(500, new { message = "An error occurred retrieving statistics" });
        }
    }

    /// <summary>
    /// Check if a profile exists for the current user
    /// </summary>
    /// <returns>Boolean indicating if profile exists</returns>
    [HttpGet("exists")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> CheckProfileExists()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var exists = await _userProfileService.ProfileExistsAsync(userId);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if profile exists");
            return StatusCode(500, new { message = "An error occurred checking profile existence" });
        }
    }
}
