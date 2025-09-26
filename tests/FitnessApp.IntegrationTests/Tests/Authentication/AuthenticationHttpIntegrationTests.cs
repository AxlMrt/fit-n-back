using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace FitnessApp.IntegrationTests.Tests.Authentication;

/// <summary>
/// Tests d'intégration HTTP pour le module Authentication - Pipeline complet Controller → Service → Repository → Database
/// Simule de vrais utilisateurs avec des requêtes JSON string comme dans un client web/mobile
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
        // Arrange - JSON string comme un vrai utilisateur l'enverrait
        var registerJson = """
        {
            "email": "newuser@example.com",
            "userName": "newuser456",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"accessToken\":");
        content.Should().Contain("\"refreshToken\":");
        content.Should().Contain("\"userId\":");
        content.Should().Contain("\"email\":\"newuser@example.com\"");
        content.Should().Contain("\"role\":\"Athlete\"");
        
        // Vérifier que les tokens sont valides
        var authResponse = JsonSerializer.Deserialize<JsonElement>(content);
        var accessToken = authResponse.GetProperty("accessToken").GetString();
        var refreshToken = authResponse.GetProperty("refreshToken").GetString();
        
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange - Premier utilisateur
        var firstUserJson = """
        {
            "email": "duplicate@example.com",
            "userName": "first456",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(firstUserJson, Encoding.UTF8, "application/json"));

        // Deuxième utilisateur avec même email
        var duplicateJson = """
        {
            "email": "duplicate@example.com",
            "userName": "second456",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(duplicateJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Email is already registered");
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ShouldReturnConflict()
    {
        // Arrange - Premier utilisateur
        var firstUserJson = """
        {
            "email": "first@example.com",
            "userName": "duplicate456",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(firstUserJson, Encoding.UTF8, "application/json"));

        // Deuxième utilisateur avec même username
        var duplicateJson = """
        {
            "email": "second@example.com",
            "userName": "duplicate456",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(duplicateJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Username is already taken");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidEmailJson = """
        {
            "email": "not-an-email",
            "userName": "validuser",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(invalidEmailJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var mismatchedPasswordsJson = """
        {
            "email": "test@example.com",
            "userName": "testuser",
            "password": "SecurePass#2024!",
            "confirmPassword": "DifferentPass#789@"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(mismatchedPasswordsJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange - D'abord créer un utilisateur
        var registerJson = """
        {
            "email": "logintest@example.com",
            "userName": "logintest",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));

        // Tentative de connexion
        var loginJson = """
        {
            "email": "logintest@example.com",
            "password": "SecurePass#2024!"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/login", 
            new StringContent(loginJson, Encoding.UTF8, "application/json"));

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
        // Arrange
        var loginJson = """
        {
            "email": "nonexistent@example.com",
            "password": "AnyPassword#2024!"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/login", 
            new StringContent(loginJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange - Créer utilisateur d'abord
        var registerJson = """
        {
            "email": "passwordtest@example.com",
            "userName": "passwordtest",
            "password": "CorrectPass#2024!",
            "confirmPassword": "CorrectPass#2024!"
        }
        """;
        
        await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));

        // Tentative avec mauvais mot de passe
        var loginJson = """
        {
            "email": "passwordtest@example.com",
            "password": "WrongPassword#789@"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/login", 
            new StringContent(loginJson, Encoding.UTF8, "application/json"));

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
        // Arrange - Créer et connecter utilisateur
        var registerJson = """
        {
            "email": "refreshtest@example.com",
            "userName": "refreshtest",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var originalRefreshToken = authData.GetProperty("refreshToken").GetString();
        var originalAccessToken = authData.GetProperty("accessToken").GetString();

        // Attendre un peu pour s'assurer que le timestamp du JWT est différent
        await Task.Delay(1100);

        // Act - Utiliser le refresh token
        var refreshJson = $$"""
        {
            "refreshToken": "{{originalRefreshToken}}"
        }
        """;

        var refreshResponse = await Client.PostAsync("/api/v1/auth/refresh", 
            new StringContent(refreshJson, Encoding.UTF8, "application/json"));

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
        // Arrange - Utiliser un token complètement invalide
        var invalidRefreshJson = """
        {
            "refreshToken": "invalid-token-that-does-not-exist"
        }
        """;

        // Act
        var response = await Client.PostAsync("/api/v1/auth/refresh", 
            new StringContent(invalidRefreshJson, Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid or expired refresh token");
    }

    [Fact]
    public async Task RefreshToken_WithUsedToken_ShouldReturnUnauthorized()
    {
        // Arrange - Créer utilisateur et obtenir tokens
        var registerJson = """
        {
            "email": "reusedtoken@example.com",
            "userName": "reusedtoken",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var originalRefreshToken = authData.GetProperty("refreshToken").GetString();

        // Première utilisation du refresh token (devrait réussir)
        var refreshJson = $$"""
        {
            "refreshToken": "{{originalRefreshToken}}"
        }
        """;

        var firstRefreshResponse = await Client.PostAsync("/api/v1/auth/refresh", 
            new StringContent(refreshJson, Encoding.UTF8, "application/json"));
        
        // Vérifier que la première utilisation fonctionne
        firstRefreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Attendre un court délai pour s'assurer que le token est bien marqué comme "Used" en BDD
        await Task.Delay(100);

        // Deuxième utilisation avec le même token original (devrait échouer car marqué comme "Used")
        var secondRefreshResponse = await Client.PostAsync("/api/v1/auth/refresh", 
            new StringContent(refreshJson, Encoding.UTF8, "application/json"));

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
        // Arrange - Créer et connecter utilisateur
        var registerJson = """
        {
            "email": "authuser@example.com",
            "userName": "authuser",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var accessToken = authData.GetProperty("accessToken").GetString();

        // Set authorization header
        SetAuthorizationHeader(accessToken!);

        // Act
        var response = await Client.GetAsync("/api/v1/auth/me");

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
        // Act - Pas de token d'autorisation
        var response = await Client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuthUser_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        SetAuthorizationHeader("invalid-jwt-token-123");

        // Act
        var response = await Client.GetAsync("/api/v1/auth/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_ShouldSucceed()
    {
        // Arrange - Créer et connecter utilisateur
        var registerJson = """
        {
            "email": "logouttest@example.com",
            "userName": "logouttest",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var accessToken = authData.GetProperty("accessToken").GetString();

        SetAuthorizationHeader(accessToken!);

        // Act - Logout
        var logoutResponse = await Client.PostAsync("/api/v1/auth/logout", null);

        // Assert
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var logoutContent = await logoutResponse.Content.ReadAsStringAsync();
        logoutContent.Should().Contain("Logged out successfully");
    }

    [Fact]
    public async Task Logout_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act - Pas de token d'autorisation
        var response = await Client.PostAsync("/api/v1/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Complete Authentication Flow Tests

    [Fact]
    public async Task CompleteAuthFlow_RegisterLoginRefreshLogout_ShouldWorkEndToEnd()
    {
        // 1. Register
        var registerJson = """
        {
            "email": "complete@example.com",
            "userName": "complete",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));
        
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. Login
        var loginJson = """
        {
            "email": "complete@example.com",
            "password": "SecurePass#2024!"
        }
        """;

        var loginResponse = await Client.PostAsync("/api/v1/auth/login", 
            new StringContent(loginJson, Encoding.UTF8, "application/json"));
        
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginData = JsonSerializer.Deserialize<JsonElement>(loginContent);
        var accessToken = loginData.GetProperty("accessToken").GetString();
        var refreshToken = loginData.GetProperty("refreshToken").GetString();

        // 3. Protected request
        SetAuthorizationHeader(accessToken!);
        var meResponse = await Client.GetAsync("/api/v1/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Refresh token
        var refreshJson = $$"""
        {
            "refreshToken": "{{refreshToken}}"
        }
        """;

        var refreshResponse = await Client.PostAsync("/api/v1/auth/refresh", 
            new StringContent(refreshJson, Encoding.UTF8, "application/json"));
        
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Logout
        var logoutResponse = await Client.PostAsync("/api/v1/auth/logout", null);
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 6. Verify token is invalidated (if token revocation is implemented)
        var finalMeResponse = await Client.GetAsync("/api/v1/auth/me");
        finalMeResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenSecurityValidation_ShouldWork()
    {
        // Arrange - Créer utilisateur
        var registerJson = """
        {
            "email": "security@example.com",
            "userName": "security",
            "password": "SecurePass#2024!",
            "confirmPassword": "SecurePass#2024!"
        }
        """;
        
        var registerResponse = await Client.PostAsync("/api/v1/auth/register", 
            new StringContent(registerJson, Encoding.UTF8, "application/json"));
        
        var registerContent = await registerResponse.Content.ReadAsStringAsync();
        var authData = JsonSerializer.Deserialize<JsonElement>(registerContent);
        var accessToken = authData.GetProperty("accessToken").GetString();

        // Act & Assert - Token validation scenarios
        
        // 1. Valid token should work
        SetAuthorizationHeader(accessToken!);
        var validResponse = await Client.GetAsync("/api/v1/auth/me");
        validResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Malformed token should fail
        SetAuthorizationHeader("malformed-token");
        var malformedResponse = await Client.GetAsync("/api/v1/auth/me");
        malformedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 3. Empty token should fail
        SetAuthorizationHeader("");
        var emptyResponse = await Client.GetAsync("/api/v1/auth/me");
        emptyResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // 4. No authorization header should fail
        Client.DefaultRequestHeaders.Authorization = null;
        var noHeaderResponse = await Client.GetAsync("/api/v1/auth/me");
        noHeaderResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
