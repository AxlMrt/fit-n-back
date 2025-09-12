using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessApp.API.Controllers.v1;

/// <summary>
/// Authentication controller handling login, registration, password management, and account operations.
/// Profile-related operations are handled by the UserProfileController.
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Authenticate a user with email/username and password.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <remarks>
    /// Successful response returns authentication tokens and user info. Example:
    /// {
    ///   "accessToken": "eyJhb...",
    ///   "refreshToken": "def456...",
    ///   "expiresIn": 3600
    /// }
    /// Error responses use: { "message": "..." }
    /// </remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)] // success: auth response (tokens + user info)
    [ProducesResponseType(typeof(object), 400)] // bad request (validation errors)
    [ProducesResponseType(typeof(object), 401)] // unauthorized (invalid credentials)
    [ProducesResponseType(typeof(object), 500)] // server error
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", request.Email);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Register a new user account.
    /// </summary>
    /// <param name="request">Registration details.</param>
    /// <remarks>
    /// On success returns created authentication data (tokens + user info).
    /// Example success body: { "accessToken":"...", "refreshToken":"...", "userId":"..." }
    /// </remarks>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 201)] // created
    [ProducesResponseType(typeof(object), 400)] // validation errors
    [ProducesResponseType(typeof(object), 409)] // conflict: email or username already exists
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Check if user already exists
            if (await _authService.ExistsWithEmailAsync(request.Email))
            {
                return Conflict(new { message = "Email is already registered" });
            }

            if (await _authService.ExistsWithUsernameAsync(request.UserName))
            {
                return Conflict(new { message = "Username is already taken" });
            }

            var response = await _authService.RegisterAsync(request);
            _logger.LogInformation("User {Email} registered successfully", request.Email);
            return CreatedAtAction(nameof(GetAuthUser), new { }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    /// <param name="request">Refresh token request.</param>
    /// <remarks>
    /// Returns new access token and optionally a new refresh token.
    /// </remarks>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)] // success: new tokens
    [ProducesResponseType(typeof(object), 400)] // bad request
    [ProducesResponseType(typeof(object), 401)] // invalid/expired refresh token
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            if (response == null)
            {
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Logout user and invalidate tokens.
    /// </summary>
    /// <remarks>
    /// Requires Authorization header with Bearer token. Returns simple success message.
    /// </remarks>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            await _authService.LogoutAsync(userId, token ?? string.Empty);
            
            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get current authenticated user information.
    /// </summary>
    /// <remarks>
    /// Returns user profile and authentication-related metadata.
    /// </remarks>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetAuthUser()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var authUser = await _authService.GetAuthUserAsync(userId);
            return Ok(authUser);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auth user");
            return NotFound(new { message = "User not found" });
        }
    }

    /// <summary>
    /// Request password reset email.
    /// </summary>
    /// <param name="request">Email for password reset.</param>
    /// <remarks>
    /// For security the response is always 200 with a generic message to avoid disclosing whether the email exists.
    /// </remarks>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            await _authService.ForgotPasswordAsync(request);
            return Ok(new { message = "If your email is registered, you will receive password reset instructions" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return Ok(new { message = "If your email is registered, you will receive password reset instructions" });
        }
    }

    /// <summary>
    /// Reset password using reset token.
    /// </summary>
    /// <param name="request">Reset password request with token.</param>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(new { message = "Password has been reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Change current password.
    /// </summary>
    /// <param name="request">Current and new password.</param>
    [HttpPut("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            await _authService.ChangePasswordAsync(userId, request);
            _logger.LogInformation("User {UserId} changed password successfully", userId);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update user email address.
    /// </summary>
    /// <param name="request">New email address.</param>
    [HttpPut("update-email")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 409)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            if (await _authService.ExistsWithEmailAsync(request.NewEmail))
            {
                return Conflict(new { message = "Email is already in use" });
            }

            await _authService.UpdateEmailAsync(userId, request);
            _logger.LogInformation("User {UserId} updated email successfully", userId);
            return Ok(new { message = "Email updated successfully. Please check your new email for confirmation." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update username.
    /// </summary>
    /// <param name="request">New username.</param>
    [HttpPut("username")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 409)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            if (await _authService.ExistsWithUsernameAsync(request.NewUsername))
            {
                return Conflict(new { message = "Username is already taken" });
            }

            await _authService.UpdateUsernameAsync(userId, request);
            _logger.LogInformation("User {UserId} updated username successfully", userId);
            return Ok(new { message = "Username updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating username");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Confirm email address.
    /// </summary>
    /// <param name="request">Email confirmation token.</param>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        try
        {
            await _authService.ConfirmEmailAsync(request);
            return Ok(new { message = "Email confirmed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming email");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Resend email confirmation.
    /// </summary>
    /// <param name="request">Email to resend confirmation.</param>
    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request)
    {
        try
        {
            await _authService.ResendEmailConfirmationAsync(request);
            return Ok(new { message = "Confirmation email sent if the email is registered" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email confirmation");
            return Ok(new { message = "Confirmation email sent if the email is registered" });
        }
    }

    /// <summary>
    /// Get security status and settings.
    /// </summary>
    [HttpGet("security")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> GetSecurityStatus()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            var securityStatus = await _authService.GetSecurityStatusAsync(userId);
            return Ok(securityStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving security status");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Enable two-factor authentication.
    /// </summary>
    [HttpPost("two-factor/enable")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> EnableTwoFactor()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            await _authService.EnableTwoFactorAsync(userId);
            return Ok(new { message = "Two-factor authentication enabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling two-factor authentication");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Disable two-factor authentication.
    /// </summary>
    [HttpPost("two-factor/disable")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> DisableTwoFactor()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            await _authService.DisableTwoFactorAsync(userId);
            return Ok(new { message = "Two-factor authentication disabled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling two-factor authentication");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate user account (admin or user themselves).
    /// </summary>
    [HttpPost("deactivate")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> DeactivateAccount()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(new { message = "Invalid user identifier" });
            }

            await _authService.DeactivateAccountAsync(userId);
            _logger.LogInformation("User {UserId} deactivated their account", userId);
            return Ok(new { message = "Account deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating account");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Reactivate user account (admin only).
    /// </summary>
    /// <param name="userId">User ID to reactivate.</param>
    [HttpPost("{userId:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> ReactivateAccount(Guid userId)
    {
        try
        {
            await _authService.ReactivateAccountAsync(userId);
            _logger.LogInformation("Admin reactivated user {UserId}", userId);
            return Ok(new { message = "Account reactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating account {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Unlock user account (admin only).
    /// </summary>
    /// <param name="userId">User ID to unlock.</param>
    [HttpPost("{userId:guid}/unlock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 403)]
    [ProducesResponseType(typeof(object), 500)]
    public async Task<IActionResult> UnlockAccount(Guid userId)
    {
        try
        {
            await _authService.UnlockAccountAsync(userId);
            _logger.LogInformation("Admin unlocked user {UserId}", userId);
            return Ok(new { message = "Account unlocked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking account {UserId}", userId);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if email exists (for registration validation).
    /// </summary>
    /// <param name="email">Email to check.</param>
    [HttpGet("exists/email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> CheckEmailExists([FromQuery] string email)
    {
        try
        {
            var exists = await _authService.ExistsWithEmailAsync(email);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if email exists");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Check if username exists (for registration validation).
    /// </summary>
    /// <param name="username">Username to check.</param>
    [HttpGet("exists/username")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(object), 400)]
    public async Task<IActionResult> CheckUsernameExists([FromQuery] string username)
    {
        try
        {
            var exists = await _authService.ExistsWithUsernameAsync(username);
            return Ok(new { exists });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if username exists");
            return BadRequest(new { message = ex.Message });
        }
    }
}
