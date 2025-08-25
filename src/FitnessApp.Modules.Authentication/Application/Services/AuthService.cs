using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.Modules.Authorization.Enums;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FitnessApp.SharedKernel.DTOs.Auth.Responses;
using FitnessApp.SharedKernel.Interfaces;

namespace FitnessApp.Modules.Authentication.Application.Services;

public class AuthService : IAuthService
{
    private readonly IValidationService _validationService;
    private readonly Users.Domain.Repositories.IUserRepository _userRepository;
    private readonly IGenerateJwtTokenService _jwtService;
    private readonly ITokenRevocationService _revocationService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        IValidationService validationService,
        Users.Domain.Repositories.IUserRepository userRepository,
        IGenerateJwtTokenService jwtService,
        ITokenRevocationService revocationService,
        IRefreshTokenService refreshTokenService)
    {
        _validationService = validationService;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _revocationService = revocationService;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        await _validationService.ValidateAsync(request);

        if (await _userRepository.ExistsWithEmailAsync(request.Email))
            throw new Exception("Email is already in use");
        if (await _userRepository.ExistsWithUsernameAsync(request.UserName))
            throw new Exception("Username is already in use");

        var user = new User(
            Email.Create(request.Email), 
            Username.Create(request.UserName));
        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword(request.Password));

        await _userRepository.AddAsync(user);

        string access = _jwtService.GenerateJwtToken(
            user.Id, 
            user.Username.Value, 
            user.Email.Value, 
            user.Role, 
            user.Subscription?.Level);
        var (refresh, refreshExp) = await _refreshTokenService.IssueAsync(user.Id);
        return new AuthResponse(
            user.Id, 
            user.Username.Value, 
            user.Email.Value, 
            access, 
            DateTime.UtcNow.AddDays(7), 
            user.Role,
            user.Subscription?.Level,
            refresh, 
            refreshExp);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        await _validationService.ValidateAsync(request);
        var user = await _userRepository.GetByEmailAsync(request.Email) ?? throw new Exception("User not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.IncrementAccessFailedCount();
            if (user.AccessFailedCount >= 5) user.LockAccount(TimeSpan.FromMinutes(15));
            await _userRepository.UpdateAsync(user);
            return null;
        }

        user.ResetAccessFailedCount();
        user.RegisterLogin();
        await _userRepository.UpdateAsync(user);

        string access = _jwtService.GenerateJwtToken(
            user.Id, 
            user.Username.Value, 
            user.Email.Value, 
            user.Role, 
            user.Subscription?.Level);
        var (refresh, refreshExp) = await _refreshTokenService.IssueAsync(user.Id);
        return new AuthResponse(
            user.Id, 
            user.Username.Value, 
            user.Email.Value, 
            access, 
            DateTime.UtcNow.AddDays(7), 
            user.Role,
            user.Subscription?.Level,
            refresh, 
            refreshExp);
    }

    public async Task LogoutAsync(Guid userId, string accessToken)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(accessToken)) return;
        await _revocationService.RevokeTokenAsync(userId, accessToken);
    }

    public async Task<AuthResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var userId = await _refreshTokenService.ValidateAsync(request.RefreshToken);
        if (userId is null) return null;

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user is null) return null;

        await _refreshTokenService.InvalidateAsync(request.RefreshToken);
        string access = _jwtService.GenerateJwtToken(
            user.Id, 
            user.Username.Value, 
            user.Email.Value, 
            user.Role, 
            user.Subscription?.Level);
        var (newRefresh, refreshExp) = await _refreshTokenService.IssueAsync(user.Id);
        return new AuthResponse(
            user.Id, 
            user.Username.Value, 
            user.Email.Value, 
            access, 
            DateTime.UtcNow.AddDays(7), 
            user.Role,
            user.Subscription?.Level,
            newRefresh, 
            refreshExp);
    }

    public Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        return Task.CompletedTask;
    }

    public Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        return Task.CompletedTask;
    }
}
