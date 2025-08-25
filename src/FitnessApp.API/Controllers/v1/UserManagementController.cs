using FitnessApp.Modules.Authorization.Enums;
using FitnessApp.Modules.Authorization.Policies;
using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.Modules.Users.Application.Services;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/users")]
public class UserManagementController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionService _subscriptionService;

    public UserManagementController(
        IUserService userService,
        IUserRepository userRepository,
        ISubscriptionService subscriptionService)
    {
        _userService = userService;
        _userRepository = userRepository;
        _subscriptionService = subscriptionService;
    }

    [HttpPut("{userId:guid}/role")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> UpdateUserRole(Guid userId, [FromBody] UpdateUserRoleRequest request)
    {
        try
        {
            var updatedUser = await _userService.UpdateUserRoleAsync(userId, request.Role);
            return Ok(new { message = $"User role updated to {request.Role}", user = updatedUser });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{userId:guid}/subscription")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> CreateSubscription(Guid userId, [FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            var subscriptionId = await _subscriptionService.CreateSubscriptionAsync(
                userId,
                request.Level,
                request.StartDate,
                request.EndDate);

            return Ok(new { id = subscriptionId, message = "Subscription created successfully" });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{userId:guid}/subscription")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> UpdateSubscription(Guid userId, [FromBody] UpdateSubscriptionRequest request)
    {
        try
        {
            var result = await _subscriptionService.UpdateSubscriptionAsync(
                userId,
                request.Level,
                request.EndDate);

            if (result)
            {
                return Ok(new { message = "Subscription updated successfully" });
            }
            else
            {
                return BadRequest(new { message = "Failed to update subscription" });
            }
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{userId:guid}/subscription")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public async Task<IActionResult> CancelSubscription(Guid userId)
    {
        try
        {
            var result = await _subscriptionService.CancelSubscriptionAsync(userId);
            
            if (result)
            {
                return Ok(new { message = "Subscription cancelled successfully" });
            }
            else
            {
                return NotFound(new { message = "No active subscription found" });
            }
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{userId:guid}/subscription")]
    [Authorize]
    public async Task<IActionResult> GetSubscription(Guid userId)
    {
        // Allow admins to access any user's subscription, but regular users can only access their own
        if (userId != Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty) &&
            !User.IsInRole(Role.Admin.ToString()))
        {
            return Forbid();
        }

        var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(userId);
        
        if (subscription == null)
        {
            return NotFound(new { message = "No subscription found" });
        }

        return Ok(subscription);
    }
}
