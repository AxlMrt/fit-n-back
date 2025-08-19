using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Application.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.API.Controllers.UsersModule;

[Authorize]
[Route("api/user")] // user-centric base route
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
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        var user = await _userService.GetUserByIdAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest profileDto)
    {
        var userId = GetUserId();
        var updated = await _userService.UpdateUserProfileAsync(userId, profileDto);
        return Ok(updated);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] PreferencesUpdateRequest request)
    {
        var userId = GetUserId();
        await _userService.UpdatePreferencesAsync(userId, request);
        return NoContent();
    }

    [HttpGet("goals")]
    public async Task<ActionResult<UserGoalsResponse>> GetGoals()
    {
        var userId = GetUserId();
        var goals = await _userService.GetGoalsAsync(userId);
        return Ok(goals);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<UserStatsResponse>> GetStats()
    {
        var userId = GetUserId();
        var stats = await _userService.GetStatsAsync(userId);
        return Ok(stats);
    }

    private Guid GetUserId()
    {
        var claim = User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : throw new UnauthorizedAccessException("Invalid user context");
    }
}