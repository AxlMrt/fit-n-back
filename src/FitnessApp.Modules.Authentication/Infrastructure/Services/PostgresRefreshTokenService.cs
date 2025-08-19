using System.Security.Cryptography;
using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.Modules.Authentication.Domain.Entities;
using FitnessApp.Modules.Authentication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Authentication.Infrastructure.Services;

public class PostgresRefreshTokenService : IRefreshTokenService
{
    private readonly AuthDbContext _db;
    public PostgresRefreshTokenService(AuthDbContext db) => _db = db;

    public async Task<(string token, DateTime expiresAt)> IssueAsync(Guid userId, TimeSpan? lifetime = null)
    {
        var (token, exp) = GenerateToken(lifetime);
        _db.RefreshTokens.Add(new RefreshToken(userId, token, exp));
        await _db.SaveChangesAsync();
        return (token, exp);
    }

    public async Task<Guid?> ValidateAsync(string token)
    {
        var rt = await _db.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(x => x.Token == token);
        if (rt is null || !rt.IsActive()) return null;
        return rt.UserId;
    }

    public async Task InvalidateAsync(string token)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);
        if (rt is null) return;
        rt.Revoke();
        await _db.SaveChangesAsync();
    }

    public async Task<(string newToken, DateTime expiresAt, Guid? userId)> RotateAsync(string oldToken, TimeSpan? lifetime = null)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == oldToken);
        if (rt is null || !rt.IsActive()) return (string.Empty, DateTime.MinValue, null);
        rt.Revoke();
        var (token, exp) = GenerateToken(lifetime);
        _db.RefreshTokens.Add(new RefreshToken(rt.UserId, token, exp));
        await _db.SaveChangesAsync();
        return (token, exp, rt.UserId);
    }

    private static (string token, DateTime exp) GenerateToken(TimeSpan? lifetime)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(bytes);
        var exp = DateTime.UtcNow.Add(lifetime ?? TimeSpan.FromDays(14));
        return (token, exp);
    }
}
