using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.SharedKernel.DTOs.Users.Requests;
using FitnessApp.SharedKernel.DTOs.Users.Responses;
using FitnessApp.SharedKernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public class UserProfileController : ControllerBase
{
    private readonly IUserProfileService _userProfileService;
    private readonly IUserPreferenceService _userPreferenceService;

    public UserProfileController(
        IUserProfileService userProfileService,
        IUserPreferenceService userPreferenceService)
    {
        _userProfileService = userProfileService ?? throw new ArgumentNullException(nameof(userProfileService));
        _userPreferenceService = userPreferenceService ?? throw new ArgumentNullException(nameof(userPreferenceService));
    }

    #region Profile Management

    /// <summary>
    /// Get the current user's profile.
    /// </summary>
    /// <returns>User profile information.</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        
        var profile = await _userProfileService.GetUserProfileAsync(userId, cancellationToken);
        
        if (profile == null)
        {
            return NotFound(new { message = "Profile not found" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Get a user profile summary by user ID.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>User profile summary</returns>
    [HttpGet("{userId:guid}/profile/summary")]
    [ProducesResponseType(typeof(UserProfileSummaryResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetProfileSummary(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _userProfileService.GetUserProfileSummaryAsync(userId, cancellationToken);
        
        if (profile == null)
        {
            return NotFound(new { message = "Profile not found" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Create a new user profile.
    /// </summary>
    /// <param name="request">Profile creation request</param>
    /// <returns>Created profile</returns>
    [HttpPost("profile")]
    [ProducesResponseType(typeof(UserProfileResponse), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 409)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Check if profile already exists
            if (await _userProfileService.UserProfileExistsAsync(userId, cancellationToken))
            {
                return Conflict(new { message = "Profile already exists" });
            }

            var profile = await _userProfileService.CreateUserProfileAsync(userId, request, cancellationToken);
            
            return CreatedAtAction(nameof(GetProfile), new { }, profile);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update personal information.
    /// </summary>
    /// <param name="request">Personal info update request</param>
    /// <returns>Updated profile</returns>
    [HttpPatch("profile/personal")]
    [ProducesResponseType(typeof(UserProfileResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePersonalInfo([FromBody] UpdatePersonalInfoRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userProfileService.UpdatePersonalInfoAsync(userId, request, cancellationToken);
            
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update physical measurements.
    /// </summary>
    /// <param name="request">Physical measurements update request</param>
    /// <returns>Updated profile</returns>
    [HttpPatch("profile/measurements")]
    [ProducesResponseType(typeof(UserProfileResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePhysicalMeasurements([FromBody] UpdatePhysicalMeasurementsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userProfileService.UpdatePhysicalMeasurementsAsync(userId, request, cancellationToken);
            
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update fitness profile.
    /// </summary>
    /// <param name="request">Fitness profile update request</param>
    /// <returns>Updated profile</returns>
    [HttpPatch("profile/fitness")]
    [ProducesResponseType(typeof(UserProfileResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateFitnessProfile([FromBody] UpdateFitnessProfileRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userProfileService.UpdateFitnessProfileAsync(userId, request, cancellationToken);
            
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete the current user's profile.
    /// </summary>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("profile")]
    [ProducesResponseType(typeof(ProfileOperationResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteProfile(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userProfileService.DeleteUserProfileAsync(userId, cancellationToken);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Subscription Management

    /// <summary>
    /// Get the current user's subscription.
    /// </summary>
    /// <returns>Subscription information</returns>
    [HttpGet("subscription")]
    [ProducesResponseType(typeof(SubscriptionResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSubscription(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        
        var subscription = await _userProfileService.GetUserSubscriptionAsync(userId, cancellationToken);
        
        if (subscription == null)
        {
            return NotFound(new { message = "No subscription found" });
        }

        return Ok(subscription);
    }

    /// <summary>
    /// Update or create subscription.
    /// </summary>
    /// <param name="request">Subscription update request</param>
    /// <returns>Updated subscription</returns>
    [HttpPost("subscription")]
    [ProducesResponseType(typeof(SubscriptionResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateSubscription([FromBody] UpdateSubscriptionRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var subscription = await _userProfileService.UpdateSubscriptionAsync(userId, request, cancellationToken);
            
            return Ok(subscription);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel the current user's subscription.
    /// </summary>
    /// <returns>Cancellation confirmation</returns>
    [HttpDelete("subscription")]
    [ProducesResponseType(typeof(ProfileOperationResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelSubscription(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userProfileService.CancelSubscriptionAsync(userId, cancellationToken);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Renew the current user's subscription.
    /// </summary>
    /// <param name="newEndDate">New subscription end date</param>
    /// <returns>Renewed subscription</returns>
    [HttpPatch("subscription/renew")]
    [ProducesResponseType(typeof(SubscriptionResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RenewSubscription([FromBody] DateTime newEndDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var subscription = await _userProfileService.RenewSubscriptionAsync(userId, newEndDate, cancellationToken);
            
            return Ok(subscription);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Preferences Management

    /// <summary>
    /// Get all user preferences.
    /// </summary>
    /// <returns>User preferences grouped by category</returns>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(UserPreferencesResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var preferences = await _userPreferenceService.GetUserPreferencesAsync(userId, cancellationToken);
            
            return Ok(preferences);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get user preferences by category.
    /// </summary>
    /// <param name="category">Preference category</param>
    [HttpGet("preferences/{category}")]
    [ProducesResponseType(typeof(UserPreferencesResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPreferencesByCategory(string category, CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse category string to enum
            if (!Enum.TryParse<PreferenceCategory>(category, true, out var categoryEnum))
            {
                return BadRequest(new { message = $"Invalid category '{category}'. Valid categories are: {string.Join(", ", Enum.GetNames<PreferenceCategory>())}" });
            }

            var userId = GetCurrentUserId();
            var preferences = await _userPreferenceService.GetUserPreferencesByCategoryAsync(userId, categoryEnum, cancellationToken);
            
            return Ok(preferences);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create or update a preference.
    /// </summary>
    /// <param name="request">Preference create/update request</param>
    [HttpPost("preferences")]
    [ProducesResponseType(typeof(PreferenceResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CreateOrUpdatePreference([FromBody] CreateOrUpdatePreferenceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var preference = await _userPreferenceService.CreateOrUpdatePreferenceAsync(userId, request, cancellationToken);
            
            return Ok(preference);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update multiple preferences at once.
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(UserPreferencesResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var preferences = await _userPreferenceService.UpdatePreferencesAsync(userId, request, cancellationToken);
            
            return Ok(preferences);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete a specific preference.
    /// </summary>
    [HttpDelete("preferences/{category}/{key}")]
    [ProducesResponseType(typeof(ProfileOperationResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePreference(string category, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse category string to enum
            if (!Enum.TryParse<PreferenceCategory>(category, true, out var categoryEnum))
            {
                return BadRequest(new { message = $"Invalid category '{category}'. Valid categories are: {string.Join(", ", Enum.GetNames<PreferenceCategory>())}" });
            }

            var userId = GetCurrentUserId();
            var result = await _userPreferenceService.DeletePreferenceAsync(userId, categoryEnum, key, cancellationToken);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Clear all preferences.
    /// </summary>
    [HttpDelete("preferences")]
    [ProducesResponseType(typeof(ProfileOperationResponse), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ClearPreferences(CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _userPreferenceService.ClearPreferencesAsync(userId, cancellationToken);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    #endregion

    #region Utility Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value
                         ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in token");
        }

        return userId;
    }

    #endregion
}