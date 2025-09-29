using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FitnessApp.API.Infrastructure.Errors;
using FitnessApp.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitnessApp.API.Controllers.v1;

/// <summary>
/// Authentication controller handling login, registration, password management, and account operations.
/// Profile-related operations are handled by the UserProfileController.
/// </summary>
[Route("api/v1/auth")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
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
    [ProducesResponseType(typeof(ApiErrorResponse), 400)] // bad request (validation errors)
    [ProducesResponseType(typeof(ApiErrorResponse), 401)] // unauthorized (invalid credentials)
    [ProducesResponseType(typeof(ApiErrorResponse), 500)] // server error
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        return Ok(response);
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
    [ProducesResponseType(typeof(ApiErrorResponse), 400)] // validation errors
    [ProducesResponseType(typeof(ApiErrorResponse), 409)] // conflict: email or username already exists
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
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
        return CreatedAtAction(nameof(GetAuthUser), new { }, response);
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
    [ProducesResponseType(typeof(ApiErrorResponse), 400)] // bad request
    [ProducesResponseType(typeof(ApiErrorResponse), 401)] // invalid/expired refresh token
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        if (response == null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token" });
        }

        return Ok(response);
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
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(new { message = "Invalid user identifier" });
        }

        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        await _authService.LogoutAsync(userId, token ?? string.Empty);
        
        return Ok(new { message = "Logged out successfully" });
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
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    public async Task<IActionResult> GetAuthUser()
    {
        var userId = GetCurrentUserId();
        var authUser = await _authService.GetAuthUserAsync(userId);
        return Ok(authUser);
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
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request);
        return Ok(new { message = "If your email is registered, you will receive password reset instructions" });
    }

    /// <summary>
    /// Reset password using reset token.
    /// </summary>
    /// <param name="request">Reset password request with token.</param>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        await _authService.ResetPasswordAsync(request);
        return Ok(new { message = "Password has been reset successfully" });
    }

    /// <summary>
    /// Change current password.
    /// </summary>
    /// <param name="request">Current and new password.</param>
    [HttpPut("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        await _authService.ChangePasswordAsync(userId, request);
        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Update user email address.
    /// </summary>
    /// <param name="request">New email address.</param>
    [HttpPut("update-email")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailRequest request)
    {
        var userId = GetCurrentUserId();

        if (await _authService.ExistsWithEmailAsync(request.NewEmail))
        {
            return Conflict(new { message = "Email is already in use" });
        }

        await _authService.UpdateEmailAsync(userId, request);
        return Ok(new { message = "Email updated successfully. Please check your new email for confirmation." });
    }

    /// <summary>
    /// Update username.
    /// </summary>
    /// <param name="request">New username.</param>
    [HttpPut("username")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 409)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameRequest request)
    {
        var userId = GetCurrentUserId();

        if (await _authService.ExistsWithUsernameAsync(request.NewUsername))
        {
            return Conflict(new { message = "Username is already taken" });
        }

        await _authService.UpdateUsernameAsync(userId, request);
        return Ok(new { message = "Username updated successfully" });
    }

    /// <summary>
    /// Confirm email address.
    /// </summary>
    /// <param name="request">Email confirmation token.</param>
    [HttpPost("confirm-email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 404)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        await _authService.ConfirmEmailAsync(request);
        return Ok(new { message = "Email confirmed successfully" });
    }

    /// <summary>
    /// Resend email confirmation.
    /// </summary>
    /// <param name="request">Email to resend confirmation.</param>
    [HttpPost("resend-confirmation")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationRequest request)
    {
        await _authService.ResendEmailConfirmationAsync(request);
        return Ok(new { message = "Confirmation email sent if the email is registered" });
    }

    /// <summary>
    /// Get security status and settings.
    /// </summary>
    [HttpGet("security")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> GetSecurityStatus()
    {
        var userId = GetCurrentUserId();
        var securityStatus = await _authService.GetSecurityStatusAsync(userId);
        return Ok(securityStatus);
    }

    /// <summary>
    /// Enable two-factor authentication.
    /// </summary>
    [HttpPost("two-factor/enable")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> EnableTwoFactor()
    {
        var userId = GetCurrentUserId();
        await _authService.EnableTwoFactorAsync(userId);
        return Ok(new { message = "Two-factor authentication enabled successfully" });
    }

    /// <summary>
    /// Disable two-factor authentication.
    /// </summary>
    [HttpPost("two-factor/disable")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> DisableTwoFactor()
    {
        var userId = GetCurrentUserId();
        await _authService.DisableTwoFactorAsync(userId);
        return Ok(new { message = "Two-factor authentication disabled successfully" });
    }

    /// <summary>
    /// Deactivate user account (admin or user themselves).
    /// </summary>
    [HttpPost("deactivate")]
    [Authorize]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userId = GetCurrentUserId();
        await _authService.DeactivateAccountAsync(userId);
        return Ok(new { message = "Account deactivated successfully" });
    }

    /// <summary>
    /// Reactivate user account (admin only).
    /// </summary>
    /// <param name="userId">User ID to reactivate.</param>
    [HttpPost("{userId:guid}/reactivate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> ReactivateAccount(Guid userId)
    {
        await _authService.ReactivateAccountAsync(userId);
        return Ok(new { message = "Account reactivated successfully" });
    }

    /// <summary>
    /// Unlock user account (admin only).
    /// </summary>
    /// <param name="userId">User ID to unlock.</param>
    [HttpPost("{userId:guid}/unlock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    [ProducesResponseType(typeof(ApiErrorResponse), 401)]
    [ProducesResponseType(typeof(ApiErrorResponse), 403)]
    [ProducesResponseType(typeof(ApiErrorResponse), 500)]
    public async Task<IActionResult> UnlockAccount(Guid userId)
    {
        await _authService.UnlockAccountAsync(userId);
        return Ok(new { message = "Account unlocked successfully" });
    }

    /// <summary>
    /// Check if email exists (for registration validation).
    /// </summary>
    /// <param name="email">Email to check.</param>
    [HttpGet("exists/email")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> CheckEmailExists([FromQuery] string email)
    {
        var exists = await _authService.ExistsWithEmailAsync(email);
        return Ok(new { exists });
    }

    /// <summary>
    /// Check if username exists (for registration validation).
    /// </summary>
    /// <param name="username">Username to check.</param>
    [HttpGet("exists/username")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(typeof(ApiErrorResponse), 400)]
    public async Task<IActionResult> CheckUsernameExists([FromQuery] string username)
    {
        var exists = await _authService.ExistsWithUsernameAsync(username);
        return Ok(new { exists });
    }
}
