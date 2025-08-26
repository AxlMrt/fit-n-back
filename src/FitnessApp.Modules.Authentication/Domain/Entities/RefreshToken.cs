namespace FitnessApp.Modules.Authentication.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? RevokedByIpAddress { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string CreatedByIpAddress { get; private set; } = null!;

    private RefreshToken() { }

    public RefreshToken(Guid userId, string token, DateTime expiresAt, string createdByIpAddress)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        CreatedByIpAddress = createdByIpAddress;
        IsRevoked = false;
    }

    public bool IsActive() => !IsRevoked && RevokedAt is null && DateTime.UtcNow < ExpiresAt;

    public void Revoke(string? replacedByToken = null, string? revokedByIpAddress = null)
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
        RevokedByIpAddress = revokedByIpAddress;
        ReplacedByToken = replacedByToken;
    }
}
