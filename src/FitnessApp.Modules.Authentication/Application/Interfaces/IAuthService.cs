using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FitnessApp.SharedKernel.DTOs.Auth.Responses;

namespace FitnessApp.Modules.Authentication.Application.Interfaces;

/// <summary>
/// Authentication service interface handling only authentication concerns.
/// User profile management is handled by the Users module.
/// </summary>
public interface IAuthService
{
    // Core authentication operations
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task LogoutAsync(Guid userId, string accessToken);
    Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    
    // Password management
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
    
    // Account management
    Task<AuthUserDto> GetAuthUserAsync(Guid userId);
    Task UpdateEmailAsync(Guid userId, UpdateEmailRequest request);
    Task UpdateUsernameAsync(Guid userId, UpdateUsernameRequest request);
    Task ConfirmEmailAsync(ConfirmEmailRequest request);
    Task ResendEmailConfirmationAsync(ResendEmailConfirmationRequest request);
    
    // Security operations
    Task<SecurityStatusDto> GetSecurityStatusAsync(Guid userId);
    Task EnableTwoFactorAsync(Guid userId);
    Task DisableTwoFactorAsync(Guid userId);
    Task UnlockAccountAsync(Guid userId);
    Task DeactivateAccountAsync(Guid userId);
    Task ReactivateAccountAsync(Guid userId);
    
    // Validation operations
    Task<bool> ExistsWithEmailAsync(string email);
    Task<bool> ExistsWithUsernameAsync(string username);
}
