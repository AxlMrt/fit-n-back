using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.Modules.Authorization;
using FitnessApp.Modules.Authorization.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FitnessApp.Modules.Authentication.Application.Services;
public class GenerateJwtTokenService : IGenerateJwtTokenService
{
    private readonly IConfiguration _configuration;

    public GenerateJwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public string GenerateJwtToken(Guid userId, string userName, string email, Role role, SubscriptionLevel? subscriptionLevel)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role.ToString())
        };

        // Add subscription level claim if available
        if (subscriptionLevel.HasValue)
        {
            claims.Add(new Claim(FitnessAppClaimTypes.SubscriptionLevel, subscriptionLevel.Value.ToString()));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}