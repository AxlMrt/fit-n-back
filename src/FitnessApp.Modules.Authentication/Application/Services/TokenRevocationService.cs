using FitnessApp.Modules.Authentication.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace FitnessApp.Modules.Authentication.Application.Services;

public class TokenRevocationService : ITokenRevocationService
{
    private readonly IDistributedCache _cache;
    
    public TokenRevocationService(IDistributedCache cache)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }
    
    public async Task RevokeTokenAsync(Guid userId, string token)
    {
        string cacheKey = $"revoked_token:{token}";
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(8) // One day more than token lifetime
        };

        await _cache.SetStringAsync(cacheKey, userId.ToString(), cacheOptions);
    }
    
    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        string cacheKey = $"revoked_token:{token}";
        string? result = await _cache.GetStringAsync(cacheKey);

        return !string.IsNullOrEmpty(result);
    }
}