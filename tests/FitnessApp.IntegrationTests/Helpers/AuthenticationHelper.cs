using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using FitnessApp.SharedKernel.DTOs.Auth.Responses;

namespace FitnessApp.IntegrationTests.Helpers;

public static class AuthenticationHelper
{
    private const string TestSecretKey = "2rO4vtN20xfKnM7gQLeGOlXXS9WDt5Z8a3bQ1kY2H8E";
    private const string TestIssuer = "FitnessApp";
    private const string TestAudience = "FitnessAppUsers";

    /// <summary>
    /// Generates a valid test JWT token.
    /// </summary>
    public static string GenerateTestJwtToken(
        Guid userId, 
        string email = "test@example.com", 
        IEnumerable<string>? roles = null,
        DateTime? expiry = null)
    {
        roles ??= new[] { "User" };
        expiry ??= DateTime.UtcNow.AddHours(1);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("user_id", userId.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: TestIssuer,
            audience: TestAudience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates a test user with authentication.
    /// </summary>
    public static TestUser CreateTestUser(
        string firstName = "Test", 
        string lastName = "User", 
        string email = "test@example.com",
        IEnumerable<string>? roles = null)
    {
        var userId = Guid.NewGuid();
        var token = GenerateTestJwtToken(userId, email, roles);

        return new TestUser
        {
            Id = userId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Token = token,
            Roles = roles?.ToList() ?? new List<string> { "User" }
        };
    }

    /// <summary>
    /// Creates a test admin with authentication.
    /// </summary>
    public static TestUser CreateTestAdmin(
        string firstName = "Admin", 
        string lastName = "User", 
        string email = "admin@example.com")
    {
        return CreateTestUser(firstName, lastName, email, new[] { "User", "Admin" });
    }

    /// <summary>
    /// Simulates an authentication response.
    /// </summary>
    public static AuthResponse CreateMockAuthResponse(TestUser user)
    {
        return new AuthResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Token,
            DateTime.UtcNow.AddHours(1),
            FitnessApp.SharedKernel.Enums.Role.Athlete,
            null,
            null,
            null
        );
    }

    /// <summary>
    /// Checks if a JWT token is valid (for tests).
    /// </summary>
    public static bool IsTokenValid(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(TestSecretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = TestIssuer,
                ValidateAudience = true,
                ValidAudience = TestAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts claims from a JWT token.
    /// </summary>
    public static IEnumerable<Claim> GetTokenClaims(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        return jwtToken.Claims;
    }

    /// <summary>
    /// Gets the user ID from a JWT token.
    /// </summary>
    public static Guid GetUserIdFromToken(string token)
    {
        var claims = GetTokenClaims(token);
        var userIdClaim = claims.FirstOrDefault(c => c.Type == "user_id" || c.Type == JwtRegisteredClaimNames.Sub);
        
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException("Unable to extract user ID from token");
    }
}

public class TestUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();

    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Returns the Authorization header for HTTP requests.
    /// </summary>
    public string AuthorizationHeader => $"Bearer {Token}";
}
