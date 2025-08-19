using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FitnessApp.SharedKernel.DTOs.Auth.Responses;

namespace FitnessApp.Modules.Authentication.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task LogoutAsync(Guid userId, string accessToken);
    Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
}
