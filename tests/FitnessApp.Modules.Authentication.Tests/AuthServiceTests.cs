using AwesomeAssertions;
using Moq;
using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.Modules.Authentication.Application.Services;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.SharedKernel.Interfaces;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.SharedKernel.DTOs.Auth.Requests;

namespace FitnessApp.Modules.Authentication.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IValidationService> _validator = new();
    private readonly Mock<IGenerateJwtTokenService> _jwt = new();
    private readonly Mock<ITokenRevocationService> _revocation = new();
    private readonly Mock<IRefreshTokenService> _refresh = new();

    [Fact]
    public async Task Login_Should_Return_Tokens_When_Credentials_Are_Valid()
    {
        var user = new User("john@doe.com", "john");
        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword("P@ssw0rd"));
        _users.Setup(r => r.GetByEmailAsync("john@doe.com")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateJwtToken(user.Id, user.UserName, user.Email, user.Role, It.IsAny<Authorization.Enums.SubscriptionLevel?>())).Returns("access");
        _refresh.Setup(r => r.IssueAsync(user.Id, It.IsAny<TimeSpan?>())).ReturnsAsync(("refresh", DateTime.UtcNow.AddDays(14)));

        var sut = new AuthService(_validator.Object, _users.Object, _jwt.Object, _revocation.Object, _refresh.Object);

        var resp = await sut.LoginAsync(new LoginRequest("john@doe.com", "P@ssw0rd"));

        resp.Should().NotBeNull();
        resp!.AccessToken.Should().Be("access");
        resp.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task Register_Should_Create_User_And_Return_Tokens()
    {
        _users.Setup(r => r.EmailExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _users.Setup(r => r.UserNameExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _jwt.Setup(j => j.GenerateJwtToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Authorization.Enums.Role>(), It.IsAny<Authorization.Enums.SubscriptionLevel?>())).Returns("access");
        _refresh.Setup(r => r.IssueAsync(It.IsAny<Guid>(), It.IsAny<TimeSpan?>())).ReturnsAsync(("refresh", DateTime.UtcNow.AddDays(14)));

        var sut = new AuthService(_validator.Object, _users.Object, _jwt.Object, _revocation.Object, _refresh.Object);

        var resp = await sut.RegisterAsync(new RegisterRequest("john@doe.com", "john", "P@ssw0rd", "P@ssw0rd"));

        resp.AccessToken.Should().Be("access");
        resp.RefreshToken.Should().Be("refresh");
    }

    [Fact]
    public async Task RefreshToken_Should_Issue_New_Tokens_When_Valid()
    {
        var user = new User("john@doe.com", "john");
        _refresh.Setup(r => r.ValidateAsync("valid")).ReturnsAsync(user.Id);
        _users.Setup(r => r.GetByIdAsync(user.Id)).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateJwtToken(user.Id, user.UserName, user.Email, user.Role, It.IsAny<Authorization.Enums.SubscriptionLevel?>())).Returns("access2");
        _refresh.Setup(r => r.IssueAsync(user.Id, It.IsAny<TimeSpan?>())).ReturnsAsync(("refresh2", DateTime.UtcNow.AddDays(14)));

        var sut = new AuthService(_validator.Object, _users.Object, _jwt.Object, _revocation.Object, _refresh.Object);

        var resp = await sut.RefreshTokenAsync(new RefreshTokenRequest("valid"));

        resp.Should().NotBeNull();
        resp!.AccessToken.Should().Be("access2");
        resp.RefreshToken.Should().Be("refresh2");
    }

    [Fact]
    public async Task Logout_Should_Revoke_Token()
    {
        var userId = Guid.NewGuid();
        var token = "access-token";
        var sut = new AuthService(_validator.Object, _users.Object, _jwt.Object, _revocation.Object, _refresh.Object);

        await sut.LogoutAsync(userId, token);

        _revocation.Verify(r => r.RevokeTokenAsync(userId, token), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_Should_Complete()
    {
        var sut = new AuthService(_validator.Object, _users.Object, _jwt.Object, _revocation.Object, _refresh.Object);
        await sut.ForgotPasswordAsync(new ForgotPasswordRequest("john@doe.com"));
    }

    [Fact]
    public async Task ResetPassword_Should_Complete()
    {
        var sut = new AuthService(_validator.Object, _users.Object, _jwt.Object, _revocation.Object, _refresh.Object);
        await sut.ResetPasswordAsync(new ResetPasswordRequest("john@doe.com", "token", "NewP@ssw0rd"));
    }
}
