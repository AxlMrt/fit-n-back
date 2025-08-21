using System.Collections.ObjectModel;
using FitnessApp.Modules.Authorization.Enums;

namespace FitnessApp.Modules.Users.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string UserName { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool EmailConfirmed { get; private set; }
    public string? SecurityStamp { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool LockoutEnabled { get; private set; }
    public int AccessFailedCount { get; private set; }
    public Role Role { get; private set; }
    public UserProfile Profile { get; private set; } = null!;
    public Subscription? Subscription { get; private set; }
    public Collection<Preference> Preferences { get; private set; } = new();
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() { } // EF Core constructor

    public User(string email, string userName)
    {
        Id = Guid.NewGuid();
        Email = email;
        UserName = userName;
        CreatedAt = DateTime.UtcNow;
        SecurityStamp = Guid.NewGuid().ToString();
        LockoutEnabled = true;
        Role = Role.Athlete; // Default role is Athlete
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void SetProfile(UserProfile profile)
    {
        Profile = profile;
    }

    public void UpdateSubscription(Subscription subscription)
    {
        Subscription = subscription;
    }

    public void SetRole(Role role)
    {
        Role = role;
    }

    public void AddPreference(Preference preference)
    {
        Preferences.Add(preference);
    }

    public void RegisterLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void IncrementAccessFailedCount()
    {
        AccessFailedCount++;
    }

    public void ResetAccessFailedCount()
    {
        AccessFailedCount = 0;
    }

    public void LockAccount(TimeSpan duration)
    {
        LockoutEnd = DateTime.UtcNow.Add(duration);
    }

    public void UnlockAccount()
    {
        LockoutEnd = null;
    }

    public bool IsLockedOut()
    {
        return LockoutEnabled && LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
    }

    public void UpdateSecurityStamp()
    {
        SecurityStamp = Guid.NewGuid().ToString();
    }
}