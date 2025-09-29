using FitnessApp.IntegrationTests.Infrastructure;
using System.Net;
using System.Text;
using System.Text.Json;
using FitnessApp.IntegrationTests.Helpers;
using static FitnessApp.IntegrationTests.Helpers.ApiJsonTemplates;

namespace FitnessApp.IntegrationTests.Tests.Authentication;

/// <summary>
/// Integration tests for Authentication module with complete HTTP pipeline testing.
/// </summary>
public class AuthenticationHttpIntegrationTests : IntegrationTestBase
{
    public AuthenticationHttpIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    #region Registration Tests

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUserAndReturnTokens()
    {
        var response = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("newuser@example.com", "newuser456", "SecurePass#2024!").ToStringContent());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"accessToken\":");
        content.Should().Contain("\"refreshToken\":");
        content.Should().Contain("\"userId\":");
        content.Should().Contain("\"email\":\"newuser@example.com\"");
        content.Should().Contain("\"role\":\"Athlete\"");
        
        var authResponse = JsonSerializer.Deserialize<JsonElement>(content);
        var accessToken = authResponse.GetProperty("accessToken").GetString();
        var refreshToken = authResponse.GetProperty("refreshToken").GetString();
        
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("duplicate@example.com", "first456", DefaultPassword).ToStringContent());

        var response = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("duplicate@example.com", "second456", DefaultPassword).ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email is already registered");
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldReturnConflict()
    {
        await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("first@example.com", "duplicate456", DefaultPassword).ToStringContent());

        var response = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("second@example.com", "duplicate456", DefaultPassword).ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username is already taken");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var response = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("not-an-email", "validuser", DefaultPassword).ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        var response = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("test@example.com", "testuser", DefaultPassword, "DifferentPass#789@").ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("logintest@example.com", "logintest", DefaultPassword).ToStringContent());

        var response = await Client.PostAsync(ApiEndpoints.Auth.Login, 
            LoginUser("logintest@example.com", DefaultPassword).ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"accessToken\":");
        content.Should().Contain("\"refreshToken\":");
        content.Should().Contain("\"email\":\"logintest@example.com\"");
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        var response = await Client.PostAsync(ApiEndpoints.Auth.Login, 
            LoginUser("nonexistent@example.com", "AnyPassword#2024!").ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("passwordtest@example.com", "passwordtest", "CorrectPass#2024!").ToStringContent());

        var response = await Client.PostAsync(ApiEndpoints.Auth.Login, 
            LoginUser("passwordtest@example.com", "WrongPassword#789@").ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid credentials");
    }

    #endregion

    #region Token Management Tests

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        var registerResponse = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("refreshtest@example.com", "refreshtest", DefaultPassword).ToStringContent());
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var originalRefreshToken = authData.GetProperty("refreshToken").GetString();
        var originalAccessToken = authData.GetProperty("accessToken").GetString();

        await Task.Delay(1100);

        var refreshResponse = await Client.PostAsync(ApiEndpoints.Auth.Refresh, 
            RefreshToken(originalRefreshToken!).ToStringContent());

        // Assert
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var refreshContent = await refreshResponse.Content.ReadAsStringAsync();
        refreshContent.Should().Contain("\"accessToken\":");
        refreshContent.Should().Contain("\"refreshToken\":");
        
        var newAuthData = JsonSerializer.Deserialize<JsonElement>(refreshContent);
        var newAccessToken = newAuthData.GetProperty("accessToken").GetString();
        var newRefreshToken = newAuthData.GetProperty("refreshToken").GetString();
        
        // Les nouveaux tokens doivent être différents des originaux
        newAccessToken.Should().NotBe(originalAccessToken);
        newRefreshToken.Should().NotBe(originalRefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var response = await Client.PostAsync(ApiEndpoints.Auth.Refresh, 
            RefreshToken("invalid-token-that-does-not-exist").ToStringContent());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RefreshToken_WithUsedToken_ShouldReturnUnauthorized()
    {
        var registerResponse = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("reusedtoken@example.com", "reusedtoken", DefaultPassword).ToStringContent());
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var originalRefreshToken = authData.GetProperty("refreshToken").GetString();

        var firstRefreshResponse = await Client.PostAsync(ApiEndpoints.Auth.Refresh, 
            RefreshToken(originalRefreshToken!).ToStringContent());
        
        // Vérifier que la première utilisation fonctionne
        firstRefreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        await Task.Delay(100);

        var secondRefreshResponse = await Client.PostAsync(ApiEndpoints.Auth.Refresh, 
            RefreshToken(originalRefreshToken!).ToStringContent());

        // Assert - Le token original ne peut plus être utilisé
        secondRefreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var secondRefreshContent = await secondRefreshResponse.Content.ReadAsStringAsync();
        secondRefreshContent.Should().Contain("Invalid or expired refresh token");
    }

    #endregion

    #region Authentication Flow Tests

    [Fact]
    public async Task GetAuthUser_WithValidToken_ShouldReturnUserInfo()
    {
        var registerResponse = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("authuser@example.com", "authuser", DefaultPassword).ToStringContent());
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var accessToken = authData.GetProperty("accessToken").GetString();

        SetAuthorizationHeader(accessToken!);

        var response = await Client.GetAsync(ApiEndpoints.Auth.Me);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"email\":\"authuser@example.com\"");
        content.Should().Contain("\"username\":\"authuser\"");
        content.Should().Contain("\"role\":\"Athlete\"");
        content.Should().Contain("\"isActive\":true");
    }

    [Fact]
    public async Task GetAuthUser_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await Client.GetAsync(ApiEndpoints.Auth.Me);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuthUser_WithInvalidToken_ShouldReturnUnauthorized()
    {
        SetAuthorizationHeader("invalid-jwt-token-123");

        var response = await Client.GetAsync(ApiEndpoints.Auth.Me);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_ShouldSucceed()
    {
        var registerResponse = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("logouttest@example.com", "logouttest", DefaultPassword).ToStringContent());
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var accessToken = authData.GetProperty("accessToken").GetString();

        SetAuthorizationHeader(accessToken!);

        var logoutResponse = await Client.PostAsync(ApiEndpoints.Auth.Logout, null);

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var logoutContent = await logoutResponse.Content.ReadAsStringAsync();
        logoutContent.Should().Contain("Logged out successfully");
    }

    [Fact]
    public async Task Logout_WithoutToken_ShouldReturnUnauthorized()
    {
        var response = await Client.PostAsync(ApiEndpoints.Auth.Logout, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Complete Authentication Flow Tests

    [Fact]
    public async Task CompleteAuthFlow_RegisterLoginRefreshLogout_ShouldWorkEndToEnd()
    {
        var registerResponse = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("complete@example.com", "complete", DefaultPassword).ToStringContent());
        
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var loginResponse = await Client.PostAsync(ApiEndpoints.Auth.Login, 
            LoginUser("complete@example.com", DefaultPassword).ToStringContent());
        
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginContent);
        var accessToken = loginData.GetProperty("accessToken").GetString();
        var refreshToken = loginData.GetProperty("refreshToken").GetString();

        SetAuthorizationHeader(accessToken!);
        var meResponse = await Client.GetAsync(ApiEndpoints.Auth.Me);
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await Client.PostAsync(ApiEndpoints.Auth.Refresh, 
            RefreshToken(refreshToken!).ToStringContent());
        
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var logoutResponse = await Client.PostAsync(ApiEndpoints.Auth.Logout, null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var finalMeResponse = await Client.GetAsync(ApiEndpoints.Auth.Me);
        finalMeResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenSecurityValidation_ShouldWork()
    {
        var registerResponse = await Client.PostAsync(ApiEndpoints.Auth.Register, 
            RegisterUser("security@example.com", "security", DefaultPassword).ToStringContent());
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var accessToken = authData.GetProperty("accessToken").GetString();

        SetAuthorizationHeader(accessToken!);
        var validResponse = await Client.GetAsync(ApiEndpoints.Auth.Me);
        validResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        SetAuthorizationHeader("malformed-token");
        var malformedResponse = await Client.GetAsync(ApiEndpoints.Auth.Me);
        malformedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        SetAuthorizationHeader("");
        var emptyResponse = await Client.GetAsync(ApiEndpoints.Auth.Me);
        emptyResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        Client.DefaultRequestHeaders.Authorization = null;
        var noHeaderResponse = await Client.GetAsync(ApiEndpoints.Auth.Me);
        noHeaderResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
