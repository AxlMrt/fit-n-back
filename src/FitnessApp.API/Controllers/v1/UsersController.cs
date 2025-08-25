using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Application.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessApp.Modules.Authorization.Policies;

namespace FitnessApp.API.Controllers.v1;

[ApiController]
[Authorize]
[Route("api/v1/user")] // user-centric base route
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            if (!Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var userDto = await _userService.GetByIdAsync(userId);
            if (userDto == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user profile");
            return StatusCode(500, new { message = "An error occurred while retrieving the user profile" });
        }
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        try
        {
            if (!Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var updatedProfile = await _userService.UpdateUserProfileAsync(userId, request);
            return Ok(updatedProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { message = "An error occurred while updating the user profile" });
        }
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesRequest request)
    {
        try
        {
            if (!Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out Guid userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            await _userService.UpdateUserPreferencesAsync(userId, request);
            return Ok(new { message = "Preferences updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences");
            return StatusCode(500, new { message = "An error occurred while updating preferences" });
        }
    }

    [HttpGet("{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
    {
        var userDto = await _userService.GetByIdAsync(userId);
        if (userDto == null)
        {
            return NotFound(new { message = "User not found" });
        }
        return Ok(userDto);
    }

    [HttpGet("by-email")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<ActionResult<UserDto>> GetUserByEmail([FromQuery] string email)
    {
        var userDto = await _userService.GetByEmailAsync(email);
        if (userDto == null)
        {
            return NotFound(new { message = "User not found" });
        }
        return Ok(userDto);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        //Seulement l'utilisateur lui-même ou un admin peut supprimer un compte
        if (!Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out Guid currentUserId) ||
            (currentUserId != userId && !User.IsInRole("Admin")))
        {
            return Forbid();
        }

        await _userService.DeactivateUserAsync(userId);
        return NoContent();
    }
}
