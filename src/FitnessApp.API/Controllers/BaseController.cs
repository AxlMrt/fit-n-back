using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitnessApp.API.Infrastructure.Errors;

namespace FitnessApp.API.Controllers;

/// <summary>
/// Base controller providing common functionality for all FitnessApp API controllers.
/// Eliminates repetitive code and ensures consistent behavior across controllers.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Gets the current authenticated user's ID from JWT claims.
    /// </summary>
    /// <returns>The user ID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated or ID is invalid</exception>
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user identifier");
        }
        
        return userId;
    }
    
    /// <summary>
    /// Gets the current authenticated user's email from JWT claims.
    /// </summary>
    /// <returns>The user email</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user is not authenticated</exception>
    protected string GetCurrentUserEmail()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        
        if (string.IsNullOrEmpty(email))
        {
            throw new UnauthorizedAccessException("User email not found in token");
        }
        
        return email;
    }
    
    /// <summary>
    /// Gets the current authenticated user's role from JWT claims.
    /// </summary>
    /// <returns>The user role</returns>
    protected string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }
    
    /// <summary>
    /// Checks if the current user is an admin.
    /// </summary>
    /// <returns>True if user is admin, false otherwise</returns>
    protected bool IsCurrentUserAdmin()
    {
        return GetCurrentUserRole()?.Equals("Admin", StringComparison.OrdinalIgnoreCase) == true;
    }
}
