namespace FitnessApp.Modules.Authentication.Infrastructure.Repositories;

public interface IRefreshTokenRepository
{
    Task<(string token, DateTime expiresAt)> IssueAsync(Guid userId, TimeSpan? lifetime = null);
    Task<Guid?> ValidateAsync(string token);
    Task InvalidateAsync(string token);
    Task<(string newToken, DateTime expiresAt, Guid? userId)> RotateAsync(string oldToken, TimeSpan? lifetime = null);
}
