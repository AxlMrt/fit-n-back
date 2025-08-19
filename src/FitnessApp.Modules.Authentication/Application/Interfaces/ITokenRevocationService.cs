namespace FitnessApp.Modules.Authentication.Application.Interfaces;

public interface ITokenRevocationService
{
    Task RevokeTokenAsync(Guid userId, string token);
    Task<bool> IsTokenRevokedAsync(string token);
}