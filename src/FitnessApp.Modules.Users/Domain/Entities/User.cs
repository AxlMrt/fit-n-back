using FitnessApp.Modules.Authorization.Enums;
using FitnessApp.Modules.Users.Domain.Exceptions;
using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.Modules.Users.Domain.Enums;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Entities;

/// <summary>
/// User aggregate root representing a user in the fitness application.
/// Encapsulates user identity, profile, security, and behavior.
/// </summary>
public class User
{
    private readonly List<Preference> _preferences = new();

    // Identity
    public Guid Id { get; private set; }
    public Email Email { get; private set; } = null!;
    public Username Username { get; private set; } = null!;
    public string PasswordHash { get; private set; } = string.Empty;
    public Role Role { get; private set; } = Role.Athlete;

    // Profile Information
    public FullName Name { get; private set; } = FullName.Empty;
    public DateOfBirth? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public PhysicalMeasurements PhysicalMeasurements { get; private set; } = PhysicalMeasurements.Empty;
    
    // Fitness Profile
    public FitnessLevel? FitnessLevel { get; private set; }
    public FitnessGoal? PrimaryFitnessGoal { get; private set; }

    // Security & Account Status
    public bool EmailConfirmed { get; private set; }
    public string SecurityStamp { get; private set; } = string.Empty;
    public bool TwoFactorEnabled { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public bool LockoutEnabled { get; private set; } = true;
    public int AccessFailedCount { get; private set; }

    // Subscription
    public Subscription? Subscription { get; private set; }

    // Collections
    public IReadOnlyList<Preference> Preferences => _preferences.AsReadOnly();

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF Core constructor
    private User() { }

    // Factory method for creating new users
    public User(Email email, Username username)
    {
        Id = Guid.NewGuid();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Username = username ?? throw new ArgumentNullException(nameof(username));
        SecurityStamp = GenerateSecurityStamp();
        CreatedAt = DateTime.UtcNow;
    }

    // Security Methods
    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new UserDomainException("Password hash cannot be empty");

        PasswordHash = passwordHash;
        UpdateSecurityStamp();
        SetUpdatedAt();
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
        
        // Auto-lockout after 5 failed attempts
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
            throw new UserDomainException("Account lockout is not enabled for this user");

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

    // Profile Methods
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
        SetUpdatedAt();
    }

    public void UpdatePersonalInfo(FullName? name, DateOfBirth? dateOfBirth, Gender? gender)
    {
        Name = name ?? FullName.Empty;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        SetUpdatedAt();
    }

    public void UpdatePhysicalMeasurements(PhysicalMeasurements measurements)
    {
        PhysicalMeasurements = measurements ?? PhysicalMeasurements.Empty;
        SetUpdatedAt();
    }

    public void UpdateFitnessProfile(FitnessLevel? fitnessLevel, FitnessGoal? primaryGoal)
    {
        FitnessLevel = fitnessLevel;
        PrimaryFitnessGoal = primaryGoal;
        SetUpdatedAt();
    }

    // Role Management
    public void SetRole(Role role)
    {
        if (Role == role)
            return;

        Role = role;
        UpdateSecurityStamp();
        SetUpdatedAt();
    }

    // Subscription Management
    public void UpdateSubscription(Subscription subscription)
    {
        Subscription = subscription;
        SetUpdatedAt();
    }

    public void RemoveSubscription()
    {
        Subscription = null;
        SetUpdatedAt();
    }

    // Preferences Management
    public void AddOrUpdatePreference(string category, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(category))
            throw new UserDomainException("Preference category is required");

        if (string.IsNullOrWhiteSpace(key))
            throw new UserDomainException("Preference key is required");

        var existingPreference = _preferences.FirstOrDefault(p => 
            p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && 
            p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        if (existingPreference != null)
        {
            existingPreference.UpdateValue(value);
        }
        else
        {
            _preferences.Add(new Preference(Id, category, key, value));
        }

        SetUpdatedAt();
    }

    public void RemovePreference(string category, string key)
    {
        var preference = _preferences.FirstOrDefault(p => 
            p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && 
            p.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        if (preference != null)
        {
            _preferences.Remove(preference);
            SetUpdatedAt();
        }
    }

    public void ClearPreferences()
    {
        _preferences.Clear();
        SetUpdatedAt();
    }

    public string? GetPreference(string category, string key)
    {
        return _preferences
            .FirstOrDefault(p => 
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase) && 
                p.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }

    // Activity Methods
    public void RegisterLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        ResetAccessFailedCount();
        SetUpdatedAt();
    }

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

    // Business Logic Queries
    public bool HasCompletedProfile()
    {
        return Name.IsComplete &&
               DateOfBirth != null &&
               Gender != null &&
               PhysicalMeasurements.HeightCm != null &&
               PhysicalMeasurements.WeightKg != null &&
               FitnessLevel != null &&
               PrimaryFitnessGoal != null;
    }

    public bool CanAccessPremiumFeatures()
    {
        return Subscription?.IsActive == true && 
               (Subscription.Level == SubscriptionLevel.Premium || Subscription.Level == SubscriptionLevel.Elite);
    }

    public bool IsCoach()
    {
        return Role == Role.Coach;
    }

    public bool IsAdmin()
    {
        return Role == Role.Admin;
    }

    public int? GetAge()
    {
        return DateOfBirth?.Age;
    }

    public decimal? GetBMI()
    {
        return PhysicalMeasurements.BMI;
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
        return $"{Guid.NewGuid():N}{DateTime.UtcNow.Ticks:X}";
    }
}