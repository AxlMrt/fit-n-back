using System.Security.Claims;
using FitnessApp.Modules.Workouts.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FitnessApp.Modules.Workouts.Application.Services;

/// <summary>
/// Implementation of current user service using HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user?.Identity?.IsAuthenticated != true)
            return null;

        // Try to get user ID from different claim types
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) 
                         ?? user.FindFirst("sub") 
                         ?? user.FindFirst("user_id")
                         ?? user.FindFirst("userId");

        if (userIdClaim?.Value != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
    }

    public IEnumerable<string> GetUserRoles()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        
        if (user?.Identity?.IsAuthenticated != true)
            return Enumerable.Empty<string>();

        return user.FindAll(ClaimTypes.Role)?.Select(c => c.Value) ?? Enumerable.Empty<string>();
    }
}
