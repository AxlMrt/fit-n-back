namespace FitnessApp.Modules.Authentication.Domain.Repositories;

public interface IRefreshTokenRepository
{
    /// <summary>
    /// Creates a new refresh token for a user
    /// </summary>
    Task<string> CreateAsync(Guid userId, TimeSpan? lifetime = null);
    
    /// <summary>
    /// Validates and marks a token as used. Returns the user ID if valid.
    /// </summary>
    Task<Guid?> ValidateAndUseAsync(string token);
    
    /// <summary>
    /// Revokes a specific refresh token
    /// </summary>
    Task RevokeAsync(string token);
    
    /// <summary>
    /// Revokes all active tokens for a user
    /// </summary>
    Task RevokeAllForUserAsync(Guid userId);
}
