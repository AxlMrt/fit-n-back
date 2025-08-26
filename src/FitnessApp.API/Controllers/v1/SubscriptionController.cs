using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.SharedKernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessApp.API.Controllers.v1;

/// <summary>
/// Subscription controller handling user subscription management.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/subscriptions")]
[Produces("application/json")]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(
        ISubscriptionService subscriptionService, 
        ILogger<SubscriptionController> logger)
    {
        _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get current user's active subscription
    /// </summary>
    /// <returns>Current subscription details</returns>
    [HttpGet("current")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetCurrentSubscription()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(userId);
            if (subscription == null)
            {
                return NotFound(new { message = "No active subscription found" });
            }

            return Ok(subscription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current subscription for user");
            return StatusCode(500, new { message = "An error occurred retrieving subscription" });
        }
    }

    /// <summary>
    /// Create a new subscription for the current user
    /// </summary>
    /// <param name="request">Subscription creation data</param>
    /// <returns>Created subscription ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            // Validate subscription level
            if (!Enum.IsDefined(typeof(SubscriptionLevel), request.Level))
            {
                return BadRequest(new { message = "Invalid subscription level" });
            }

            // Validate dates
            if (request.StartDate >= request.EndDate)
            {
                return BadRequest(new { message = "End date must be after start date" });
            }

            if (request.StartDate < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "Start date cannot be in the past" });
            }

            var subscriptionId = await _subscriptionService.CreateSubscriptionAsync(
                userId, 
                request.Level, 
                request.StartDate, 
                request.EndDate);

            _logger.LogInformation("Created subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);
            
            return CreatedAtAction(
                nameof(GetCurrentSubscription), 
                new { }, 
                new { subscriptionId, message = "Subscription created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating subscription");
            return StatusCode(500, new { message = "An error occurred creating subscription" });
        }
    }

    /// <summary>
    /// Update the current user's subscription
    /// </summary>
    /// <param name="request">Subscription update data</param>
    /// <returns>Success message</returns>
    [HttpPut]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> UpdateSubscription([FromBody] UpdateSubscriptionRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            // Validate subscription level
            if (!Enum.IsDefined(typeof(SubscriptionLevel), request.Level))
            {
                return BadRequest(new { message = "Invalid subscription level" });
            }

            // Validate end date
            if (request.EndDate < DateTime.UtcNow.Date)
            {
                return BadRequest(new { message = "End date cannot be in the past" });
            }

            var updated = await _subscriptionService.UpdateSubscriptionAsync(
                userId, 
                request.Level, 
                request.EndDate);

            if (!updated)
            {
                return NotFound(new { message = "No active subscription found to update" });
            }

            _logger.LogInformation("Updated subscription for user {UserId}", userId);
            return Ok(new { message = "Subscription updated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating subscription");
            return StatusCode(500, new { message = "An error occurred updating subscription" });
        }
    }

    /// <summary>
    /// Cancel the current user's subscription
    /// </summary>
    /// <returns>Success message</returns>
    [HttpDelete]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> CancelSubscription()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var cancelled = await _subscriptionService.CancelSubscriptionAsync(userId);
            if (!cancelled)
            {
                return NotFound(new { message = "No active subscription found to cancel" });
            }

            _logger.LogInformation("Cancelled subscription for user {UserId}", userId);
            return Ok(new { message = "Subscription cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling subscription");
            return StatusCode(500, new { message = "An error occurred cancelling subscription" });
        }
    }

    /// <summary>
    /// Check if current user has an active subscription
    /// </summary>
    /// <returns>Boolean indicating if user has active subscription</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(object), 200)]
    public async Task<IActionResult> GetSubscriptionStatus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var subscription = await _subscriptionService.GetCurrentSubscriptionAsync(userId);
            var hasActiveSubscription = subscription != null;

            return Ok(new 
            { 
                hasActiveSubscription,
                subscriptionLevel = subscription?.Level.ToString(),
                expiresAt = subscription?.EndDate
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking subscription status");
            return StatusCode(500, new { message = "An error occurred checking subscription status" });
        }
    }
}

/// <summary>
/// Request model for creating a subscription
/// </summary>
public record CreateSubscriptionRequest(
    SubscriptionLevel Level,
    DateTime StartDate,
    DateTime EndDate);

/// <summary>
/// Request model for updating a subscription
/// </summary>
public record UpdateSubscriptionRequest(
    SubscriptionLevel Level,
    DateTime EndDate);
