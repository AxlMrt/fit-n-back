using FitnessApp.SharedKernel.Enums;
using FitnessApp.Modules.Authentication.Domain.Exceptions;
using FitnessApp.Modules.Authentication.Domain.ValueObjects;
using FitnessApp.SharedKernel.ValueObjects;

namespace FitnessApp.Modules.Authentication.Domain.Entities;

/// <summary>
/// Authentication-focused user entity containing only identity and security information.
/// Separate from user profile data which belongs to the Users module.
/// </summary>
public class AuthUser
{
    // Identity
    public Guid Id { get; private set; }
    public Email Email { get; private set; } = null!;
    public Username Username { get; private set; } = null!;
    public PasswordHash PasswordHash { get; private set; } = null!;
    public string SecurityStamp { get; private set; } = string.Empty;
    public Role Role { get; private set; } = Role.Athlete;

    // Security & Account Status
    public bool EmailConfirmed { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool LockoutEnabled { get; private set; } = true;
    public int AccessFailedCount { get; private set; }

    // Password Reset
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    // Email Verification
    public string? EmailVerificationToken { get; private set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF Core constructor
    private AuthUser() { }

    // Factory method for creating new auth users
    public AuthUser(Email email, Username username, PasswordHash passwordHash)
    {
        Id = Guid.NewGuid();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Username = username ?? throw new ArgumentNullException(nameof(username));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        SecurityStamp = GenerateSecurityStamp();
        CreatedAt = DateTime.UtcNow;
        GenerateEmailVerificationToken();
    }

    // Security Methods
    public void SetPasswordHash(PasswordHash passwordHash)
    {
        if (passwordHash == null)
            throw AuthenticationDomainException.InvalidPasswordHash();

        PasswordHash = passwordHash;
        UpdateSecurityStamp();
        SetUpdatedAt();
    }

    public bool VerifyPassword(string password)
    {
        return PasswordHash?.Verify(password) == true;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        SetUpdatedAt();
    }

    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
        UpdateSecurityStamp();
        SetUpdatedAt();
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
        UpdateSecurityStamp();
        SetUpdatedAt();
    }

    // Account Lockout Methods
    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
        
        // Auto-lockout after configured failed attempts
        if (AccessFailedCount >= 5 && LockoutEnabled)
        {
            LockAccount(TimeSpan.FromMinutes(30));
        }
        
        SetUpdatedAt();
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
        SetUpdatedAt();
    }

    public void LockAccount(TimeSpan duration)
    {
        if (!LockoutEnabled)
            throw AuthenticationDomainException.LockoutNotEnabled();

        LockoutEnd = DateTime.UtcNow.Add(duration);
        SetUpdatedAt();
    }

    public void UnlockAccount()
    {
        LockoutEnd = null;
        AccessFailedCount = 0;
        SetUpdatedAt();
    }

    public bool IsLockedOut()
    {
        return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    // Authentication Methods
    public void RegisterLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        ResetAccessFailedCount();
        SetUpdatedAt();
    }

    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null)
            throw new ArgumentNullException(nameof(newEmail));

        if (Email.Equals(newEmail))
            return;

        Email = newEmail;
        EmailConfirmed = false; // Need to re-confirm new email
        UpdateSecurityStamp();
        SetUpdatedAt();
    }

    public void UpdateUsername(Username newUsername)
    {
        if (newUsername == null)
            throw new ArgumentNullException(nameof(newUsername));

        if (Username.Equals(newUsername))
            return;

        Username = newUsername;
        UpdateSecurityStamp();
        SetUpdatedAt();
    }

    // Role Management
    public void SetRole(Role role)
    {
        Role = role;
        UpdateSecurityStamp();
        SetUpdatedAt();
    }

    // Activity Methods
    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public void Reactivate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    // Private helper methods
    private void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateSecurityStamp()
    {
        SecurityStamp = GenerateSecurityStamp();
    }

    private static string GenerateSecurityStamp()
    {
        return Guid.NewGuid().ToString("N")[..16].ToUpperInvariant();
    }

    // Password Reset Token Management
    public void GeneratePasswordResetToken()
    {
        PasswordResetToken = GenerateSecureToken();
        PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1); // Valid for 1 hour
        SetUpdatedAt();
    }

    public bool IsPasswordResetTokenValid(string token)
    {
        return !string.IsNullOrEmpty(PasswordResetToken) &&
               PasswordResetToken == token &&
               PasswordResetTokenExpiresAt.HasValue &&
               PasswordResetTokenExpiresAt.Value > DateTime.UtcNow;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        SetUpdatedAt();
    }

    // Email Verification Token Management
    public void GenerateEmailVerificationToken()
    {
        EmailVerificationToken = GenerateSecureToken();
        EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddDays(7); // Valid for 7 days
        SetUpdatedAt();
    }

    public bool IsEmailVerificationTokenValid(string token)
    {
        return !string.IsNullOrEmpty(EmailVerificationToken) &&
               EmailVerificationToken == token &&
               EmailVerificationTokenExpiresAt.HasValue &&
               EmailVerificationTokenExpiresAt.Value > DateTime.UtcNow;
    }

    public void ClearEmailVerificationToken()
    {
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
        SetUpdatedAt();
    }

    private static string GenerateSecureToken()
    {
        return $"{Guid.NewGuid():N}{Guid.NewGuid():N}";
    }
}
