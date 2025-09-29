using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.Modules.Authentication.Domain.Entities;
using FitnessApp.Modules.Authentication.Domain.Exceptions;
using FitnessApp.Modules.Authentication.Domain.Repositories;
using FitnessApp.Modules.Authentication.Domain.ValueObjects;
using FitnessApp.Modules.Authentication.Infrastructure.Repositories;
using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FitnessApp.SharedKernel.DTOs.Auth.Responses;
using FitnessApp.SharedKernel.Interfaces;
using FitnessApp.SharedKernel.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Authentication.Application.Services;

/// <summary>
/// Authentication service responsible only for authentication operations.
/// Does not manage user profiles - that's handled by the Users module.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IValidationService _validationService;
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IGenerateJwtTokenService _jwtService;
    private readonly ITokenRevocationService _revocationService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IValidationService validationService,
        IAuthenticationRepository authenticationRepository,
        IGenerateJwtTokenService jwtService,
        ITokenRevocationService revocationService,
        IRefreshTokenRepository refreshTokenRepository,
        ILogger<AuthService> logger)
    {
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _authenticationRepository = authenticationRepository ?? throw new ArgumentNullException(nameof(authenticationRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _revocationService = revocationService ?? throw new ArgumentNullException(nameof(revocationService));
        _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        _logger.LogInformation("Starting user registration for email: {Email}", request.Email);
        
        await _validationService.ValidateAsync(request);

        // Check for duplicates
        if (await _authenticationRepository.ExistsWithEmailAsync(request.Email))
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            throw AuthenticationDomainException.EmailAlreadyExists(request.Email);
        }
        
        if (await _authenticationRepository.ExistsWithUsernameAsync(request.UserName))
        {
            _logger.LogWarning("Registration attempt with existing username: {UserName}", request.UserName);
            throw AuthenticationDomainException.UsernameAlreadyExists(request.UserName);
        }

        // Validate password confirmation
        if (request.Password != request.ConfirmPassword)
        {
            throw AuthenticationDomainException.PasswordMismatch();
        }

        try
        {
            // Create value objects
            var email = Email.Create(request.Email);
            var username = Username.Create(request.UserName);
            var passwordHash = PasswordHash.Create(request.Password);

            // Create authentication user
            var authUser = new AuthUser(email, username, passwordHash);

            // Save to repository
            var savedAuthUser = await _authenticationRepository.AddAsync(authUser);

            _logger.LogInformation("User registered successfully with ID: {UserId}", savedAuthUser.Id);

            // Generate tokens
            string accessToken = _jwtService.GenerateJwtToken(
                savedAuthUser.Id, 
                savedAuthUser.Username.Value, 
                savedAuthUser.Email.Value, 
                savedAuthUser.Role, 
                null); // No subscription level in auth module
                
            var refreshToken = await _refreshTokenRepository.CreateAsync(savedAuthUser.Id);

            return new AuthResponse(
                savedAuthUser.Id, 
                savedAuthUser.Username.Value, 
                savedAuthUser.Email.Value, 
                accessToken, 
                DateTime.UtcNow.AddDays(7), 
                savedAuthUser.Role,
                null, // No subscription level in auth module
                refreshToken, 
                DateTime.UtcNow.AddDays(30)); // Default refresh token expiry
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration for email: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for email: {Email}", request.Email);
        
        await _validationService.ValidateAsync(request);
        
        var authUser = await _authenticationRepository.GetByEmailAsync(request.Email);
        if (authUser == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return null; // Don't reveal whether user exists
        }

        // Check if account is active and not locked
        if (!authUser.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive account: {Email}", request.Email);
            return null;
        }

        if (authUser.IsLockedOut())
        {
            _logger.LogWarning("Login attempt for locked account: {Email}", request.Email);
            return null;
        }

        // Verify password
        if (!authUser.VerifyPassword(request.Password))
        {
            _logger.LogWarning("Invalid password attempt for email: {Email}", request.Email);
            authUser.IncrementAccessFailedCount();
            await _authenticationRepository.UpdateAsync(authUser);
            return null;
        }

        // Successful login
        authUser.ResetAccessFailedCount();
        authUser.RegisterLogin();
        await _authenticationRepository.UpdateAsync(authUser);

        _logger.LogInformation("Successful login for user: {UserId}", authUser.Id);

        // Generate tokens
        string accessToken = _jwtService.GenerateJwtToken(
            authUser.Id, 
            authUser.Username.Value, 
            authUser.Email.Value, 
            authUser.Role, 
            null); // No subscription in auth module
            
        var refreshToken = await _refreshTokenRepository.CreateAsync(authUser.Id);
        
        return new AuthResponse(
            authUser.Id, 
            authUser.Username.Value, 
            authUser.Email.Value, 
            accessToken, 
            DateTime.UtcNow.AddDays(7), 
            authUser.Role,
            null, // No subscription in auth module
            refreshToken, 
            DateTime.UtcNow.AddDays(30)); // Default refresh token expiry
    }

    public async Task LogoutAsync(Guid userId, string accessToken)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(accessToken)) return;
        await _revocationService.RevokeTokenAsync(userId, accessToken);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        _logger.LogDebug("Refresh token attempt");
        
        // Validate and use the token (marks as used if valid)
        var userId = await _refreshTokenRepository.ValidateAndUseAsync(request.RefreshToken);
        if (userId is null) 
        {
            _logger.LogWarning("Invalid, expired, or already used refresh token provided");
            return null;
        }

        var authUser = await _authenticationRepository.GetByIdAsync(userId.Value);
        if (authUser is null || !authUser.IsActive) 
        {
            _logger.LogWarning("Refresh token for invalid or inactive user: {UserId}", userId.Value);
            return null;
        }
        
        // Generate new tokens
        string accessToken = _jwtService.GenerateJwtToken(
            authUser.Id, 
            authUser.Username.Value, 
            authUser.Email.Value, 
            authUser.Role, 
            null); // No subscription in auth module

        string newRefreshToken = await _refreshTokenRepository.CreateAsync(authUser.Id);
        
        return new AuthResponse(
            authUser.Id, 
            authUser.Username.Value, 
            authUser.Email.Value, 
            accessToken, 
            DateTime.UtcNow.AddDays(7), 
            authUser.Role,
            null, // No subscription in auth module
            newRefreshToken, 
            DateTime.UtcNow.AddDays(30)); // Default refresh token expiry
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        _logger.LogInformation("Password reset requested for email: {Email}", request.Email);
        
        await _validationService.ValidateAsync(request);
        
        var authUser = await _authenticationRepository.GetByEmailAsync(request.Email);
        if (authUser == null || !authUser.IsActive)
        {
            // Don't reveal if user exists - just log and return
            _logger.LogWarning("Password reset requested for non-existent or inactive email: {Email}", request.Email);
            return; // Always return success to prevent email enumeration
        }

        // Generate password reset token (this should be added to AuthUser entity)
        // For now, just log that the request was processed
        _logger.LogInformation("Password reset token would be generated for user: {UserId}", authUser.Id);
        
        // In a real implementation:
        // 1. Generate secure token
        // 2. Set expiration time (e.g., 1 hour)
        // 3. Send email with reset link
        // 4. Save token to database
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        _logger.LogInformation("Password reset attempt for email: {Email}", request.Email);
        
        await _validationService.ValidateAsync(request);

        // Validate password confirmation
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw AuthenticationDomainException.PasswordMismatch();
        }

        var authUser = await _authenticationRepository.GetByEmailAsync(request.Email);
        if (authUser == null || !authUser.IsActive)
        {
            _logger.LogWarning("Password reset attempt for non-existent or inactive email: {Email}", request.Email);
            throw AuthenticationDomainException.InvalidToken();
        }

        // In a real implementation, validate the token here
        // For now, just update the password
        try
        {
            var newPasswordHash = PasswordHash.Create(request.NewPassword);
            authUser.SetPasswordHash(newPasswordHash);
            
            await _authenticationRepository.UpdateAsync(authUser);
            
            _logger.LogInformation("Password successfully reset for user: {UserId}", authUser.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user email: {Email}", request.Email);
            throw;
        }
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        _logger.LogInformation("Password change requested for user: {UserId}", userId);
        
        await _validationService.ValidateAsync(request);

        // Validate password confirmation
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            throw AuthenticationDomainException.PasswordMismatch();
        }

        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null || !authUser.IsActive)
        {
            _logger.LogWarning("Password change attempt for non-existent or inactive user: {UserId}", userId);
            throw AuthenticationDomainException.UserNotFound();
        }

        // Verify current password
        if (!authUser.VerifyPassword(request.CurrentPassword))
        {
            _logger.LogWarning("Invalid current password provided for user: {UserId}", userId);
            throw AuthenticationDomainException.IncorrectPassword();
        }

        try
        {
            var newPasswordHash = PasswordHash.Create(request.NewPassword);
            authUser.SetPasswordHash(newPasswordHash);
            
            await _authenticationRepository.UpdateAsync(authUser);
            
            _logger.LogInformation("Password successfully changed for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AuthUserDto> GetAuthUserAsync(Guid userId)
    {
        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        return new AuthUserDto(
            authUser.Id,
            authUser.Email.Value,
            authUser.Username.Value,
            authUser.Role,
            authUser.EmailConfirmed,
            authUser.TwoFactorEnabled,
            authUser.IsActive,
            authUser.CreatedAt,
            authUser.LastLoginAt
        );
    }

    public async Task UpdateEmailAsync(Guid userId, UpdateEmailRequest request)
    {
        _logger.LogInformation("Email update requested for user: {UserId}", userId);
        
        await _validationService.ValidateAsync(request);

        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null || !authUser.IsActive)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        // Check if new email already exists
        if (await _authenticationRepository.ExistsWithEmailAsync(request.NewEmail))
        {
            throw AuthenticationDomainException.EmailAlreadyExists(request.NewEmail);
        }

        try
        {
            var newEmail = Email.Create(request.NewEmail);
            authUser.UpdateEmail(newEmail);
            
            await _authenticationRepository.UpdateAsync(authUser);
            
            _logger.LogInformation("Email updated successfully for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email for user: {UserId}", userId);
            throw;
        }
    }

    public async Task UpdateUsernameAsync(Guid userId, UpdateUsernameRequest request)
    {
        _logger.LogInformation("Username update requested for user: {UserId}", userId);
        
        await _validationService.ValidateAsync(request);

        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null || !authUser.IsActive)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        // Check if new username already exists
        if (await _authenticationRepository.ExistsWithUsernameAsync(request.NewUsername))
        {
            throw AuthenticationDomainException.UsernameAlreadyExists(request.NewUsername);
        }

        try
        {
            var newUsername = Username.Create(request.NewUsername);
            authUser.UpdateUsername(newUsername);
            
            await _authenticationRepository.UpdateAsync(authUser);
            
            _logger.LogInformation("Username updated successfully for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating username for user: {UserId}", userId);
            throw;
        }
    }

    public async Task ConfirmEmailAsync(ConfirmEmailRequest request)
    {
        _logger.LogInformation("Email confirmation requested with token");
        
        await _validationService.ValidateAsync(request);

        var authUser = await _authenticationRepository.GetByEmailVerificationTokenAsync(request.Token);
        if (authUser == null)
        {
            throw AuthenticationDomainException.InvalidToken();
        }

        if (authUser.IsEmailVerificationTokenValid(request.Token))
        {
            authUser.ConfirmEmail();
            authUser.ClearEmailVerificationToken();
            
            await _authenticationRepository.UpdateAsync(authUser);
            
            _logger.LogInformation("Email confirmed successfully for user: {UserId}", authUser.Id);
        }
        else
        {
            throw AuthenticationDomainException.InvalidToken();
        }
    }

    public async Task ResendEmailConfirmationAsync(ResendEmailConfirmationRequest request)
    {
        _logger.LogInformation("Email confirmation resend requested for: {Email}", request.Email);
        
        await _validationService.ValidateAsync(request);

        var authUser = await _authenticationRepository.GetByEmailAsync(request.Email);
        if (authUser == null || !authUser.IsActive)
        {
            // Don't reveal if user exists
            _logger.LogWarning("Email confirmation resend for non-existent email: {Email}", request.Email);
            return;
        }

        if (authUser.EmailConfirmed)
        {
            _logger.LogInformation("Email confirmation resend requested for already confirmed email: {Email}", request.Email);
            return; // Don't resend if already confirmed
        }

        authUser.GenerateEmailVerificationToken();
        await _authenticationRepository.UpdateAsync(authUser);
        
        _logger.LogInformation("Email confirmation token regenerated for user: {UserId}", authUser.Id);
        
        // In real implementation: send email with new token
    }

    public async Task<SecurityStatusDto> GetSecurityStatusAsync(Guid userId)
    {
        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        return new SecurityStatusDto(
            authUser.EmailConfirmed,
            authUser.TwoFactorEnabled,
            authUser.IsLockedOut(),
            authUser.LockoutEnd,
            authUser.AccessFailedCount,
            authUser.LastLoginAt,
            !string.IsNullOrEmpty(authUser.PasswordResetToken),
            authUser.UpdatedAt
        );
    }

    public async Task EnableTwoFactorAsync(Guid userId)
    {
        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null || !authUser.IsActive)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        authUser.EnableTwoFactor();
        await _authenticationRepository.UpdateAsync(authUser);
        
        _logger.LogInformation("Two-factor authentication enabled for user: {UserId}", userId);
    }

    public async Task DisableTwoFactorAsync(Guid userId)
    {
        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null || !authUser.IsActive)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        authUser.DisableTwoFactor();
        await _authenticationRepository.UpdateAsync(authUser);
        
        _logger.LogInformation("Two-factor authentication disabled for user: {UserId}", userId);
    }

    public async Task UnlockAccountAsync(Guid userId)
    {
        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        authUser.UnlockAccount();
        await _authenticationRepository.UpdateAsync(authUser);
        
        _logger.LogInformation("Account unlocked for user: {UserId}", userId);
    }

    public async Task DeactivateAccountAsync(Guid userId)
    {
        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        authUser.Deactivate();
        await _authenticationRepository.UpdateAsync(authUser);
        
        _logger.LogInformation("Account deactivated for user: {UserId}", userId);
    }

    public async Task ReactivateAccountAsync(Guid userId)
    {
        var authUser = await _authenticationRepository.GetByIdAsync(userId);
        if (authUser == null)
        {
            throw AuthenticationDomainException.UserNotFound();
        }

        authUser.Reactivate();
        await _authenticationRepository.UpdateAsync(authUser);
        
        _logger.LogInformation("Account reactivated for user: {UserId}", userId);
    }

    public async Task<bool> ExistsWithEmailAsync(string email)
    {
        return await _authenticationRepository.ExistsWithEmailAsync(email);
    }

    public async Task<bool> ExistsWithUsernameAsync(string username)
    {
        return await _authenticationRepository.ExistsWithUsernameAsync(username);
    }
}
