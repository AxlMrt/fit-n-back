using System.Security.Cryptography;
using FitnessApp.Modules.Authentication.Domain.Entities;
using FitnessApp.Modules.Authentication.Domain.Repositories;
using FitnessApp.Modules.Authentication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Authentication.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthenticationDbContext _context;

    public RefreshTokenRepository(AuthenticationDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateAsync(Guid userId, TimeSpan? lifetime = null)
    {
        await RevokeAllForUserAsync(userId);

        var token = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromDays(30));
        
        var refreshToken = new RefreshToken(userId, token, expiresAt, "system");
        
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        
        return token;
    }

    public async Task<Guid?> ValidateAndUseAsync(string token)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken is null || !refreshToken.IsActive)
            {
                return null;
            }

            refreshToken.MarkAsUsed();
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return refreshToken.UserId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task RevokeAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token);

        if (refreshToken is not null)
        {
            refreshToken.Revoke("Manual revocation");
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsUsed && !x.IsRevoked && x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.Revoke("New token issued");
        }

        if (activeTokens.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
