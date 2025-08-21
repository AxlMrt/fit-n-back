using FitnessApp.Modules.Authorization.Policies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FitnessApp.API.Controllers.Examples;

[ApiController]
[Route("api/examples")]
public class AuthorizationExampleController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { message = "This endpoint is public and accessible to anyone" });
    }

    [HttpGet("authenticated")]
    public IActionResult AuthenticatedEndpoint()
    {
        return Ok(new { message = "This endpoint requires authentication" });
    }

    [HttpGet("admin")]
    [Authorize(Policy = AuthorizationPolicies.RequireAdmin)]
    public IActionResult AdminEndpoint()
    {
        return Ok(new { message = "This endpoint requires admin role" });
    }

    [HttpGet("coach")]
    [Authorize(Policy = AuthorizationPolicies.RequireCoach)]
    public IActionResult CoachEndpoint()
    {
        return Ok(new { message = "This endpoint requires coach role" });
    }

    [HttpGet("premium-content")]
    [Authorize(Policy = AuthorizationPolicies.RequirePremiumSubscription)]
    public IActionResult PremiumContentEndpoint()
    {
        return Ok(new { message = "This endpoint requires premium subscription" });
    }

    [HttpGet("elite-content")]
    [Authorize(Policy = AuthorizationPolicies.RequireEliteSubscription)]
    public IActionResult EliteContentEndpoint()
    {
        return Ok(new { message = "This endpoint requires elite subscription" });
    }

    [HttpGet("coach-premium")]
    [Authorize(Policy = AuthorizationPolicies.RequireCoachWithPremium)]
    public IActionResult CoachPremiumEndpoint()
    {
        return Ok(new { message = "This endpoint requires coach role and premium subscription" });
    }

    [HttpGet("manage-users")]
    [Authorize(Policy = AuthorizationPolicies.CanManageUsers)]
    public IActionResult ManageUsersEndpoint()
    {
        return Ok(new { message = "This endpoint allows managing users" });
    }

    [HttpGet("manage-content")]
    [Authorize(Policy = AuthorizationPolicies.CanManageContent)]
    public IActionResult ManageContentEndpoint()
    {
        return Ok(new { message = "This endpoint allows managing content" });
    }

    [HttpGet("analytics")]
    [Authorize(Policy = AuthorizationPolicies.CanAccessAdvancedAnalytics)]
    public IActionResult AnalyticsEndpoint()
    {
        return Ok(new { message = "This endpoint provides access to advanced analytics" });
    }
}
