using FitnessApp.Modules.Authentication.Domain.Exceptions;

namespace FitnessApp.Modules.Authentication.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedByIpAddress { get; private set; }
    public bool IsUsed { get; internal set; }
    public DateTime? UsedAt { get; internal set; }
    public bool IsRevoked { get; internal set; }
    public DateTime? RevokedAt { get; internal set; }
    public string? RevokedReason { get; internal set; }

    private RefreshToken() { } // EF Core

    public RefreshToken(Guid userId, string token, DateTime expiresAt, string? createdByIpAddress = null)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIpAddress = createdByIpAddress;
        IsUsed = false;
        IsRevoked = false;
    }

    public bool IsActive => !IsUsed && !IsRevoked && !IsExpired;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    public void MarkAsUsed()
    {
        if (!IsActive)
            throw AuthenticationDomainException.InactiveToken();
            
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }

    public void Revoke(string? reason = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
    }
}
