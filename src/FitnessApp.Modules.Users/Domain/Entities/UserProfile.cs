using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Entities;

public class UserProfile
{
    private readonly List<Preference> _preferences = new();

    // Identity - Foreign key to AuthUser
    public Guid UserId { get; private set; }

    // Profile Information
    public FullName Name { get; private set; } = FullName.Empty;
    public DateOfBirth? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public PhysicalMeasurements PhysicalMeasurements { get; private set; } = PhysicalMeasurements.Empty;
    // Fitness Profile
    public FitnessLevel? FitnessLevel { get; private set; }
    public FitnessGoal? PrimaryFitnessGoal { get; private set; }

    // Subscription (business logic, not authentication)
    public Subscription? Subscription { get; private set; }

    // Collections
    public IReadOnlyList<Preference> Preferences => _preferences.AsReadOnly();

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor
    private UserProfile() { }

    // Factory method for creating new user profiles
    public UserProfile(Guid userId)
    {
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

    // Profile Methods
    public void UpdatePersonalInfo(FullName? name, DateOfBirth? dateOfBirth, Gender? gender)
    {
        if (name != null) Name = name;
        if (dateOfBirth != null) DateOfBirth = dateOfBirth;
        if (gender != null) Gender = gender;
        
        SetUpdatedAt();
    }

    public void UpdatePhysicalMeasurements(PhysicalMeasurements measurements)
    {
        PhysicalMeasurements = measurements ?? throw new ArgumentNullException(nameof(measurements));
        SetUpdatedAt();
    }

    public void UpdateFitnessProfile(FitnessLevel? fitnessLevel, FitnessGoal? primaryGoal)
    {
        if (fitnessLevel != null) FitnessLevel = fitnessLevel;
        if (primaryGoal != null) PrimaryFitnessGoal = primaryGoal;
        
        SetUpdatedAt();
    }

    // Subscription Management
    public void UpdateSubscription(Subscription subscription)
    {
        Subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
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
            throw new ArgumentException("Category cannot be empty", nameof(category));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        var existingPreference = _preferences.FirstOrDefault(p => 
            p.Category == category && p.Key == key);

        if (existingPreference != null)
        {
            existingPreference.UpdateValue(value ?? string.Empty);
        }
        else
        {
            var newPreference = new Preference(UserId, category, key, value ?? string.Empty);
            _preferences.Add(newPreference);
        }
        
        SetUpdatedAt();
    }

    public void RemovePreference(string category, string key)
    {
        var preference = _preferences.FirstOrDefault(p => 
            p.Category == category && p.Key == key);

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
            .FirstOrDefault(p => p.Category == category && p.Key == key)
            ?.Value;
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

    public int? GetAge()
    {
        if (DateOfBirth?.Value == null) return null;
        
        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Value.Year;
        
        if (DateOfBirth.Value.Date > today.AddYears(-age))
            age--;
            
        return age;
    }

    public decimal? GetBMI()
    {
        if (PhysicalMeasurements.HeightCm == null || PhysicalMeasurements.WeightKg == null)
            return null;

        var heightInMeters = PhysicalMeasurements.HeightCm.Value / 100.0m;
        return PhysicalMeasurements.WeightKg.Value / (heightInMeters * heightInMeters);
    }

    // Private helper methods
    private void SetUpdatedAt()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    // Public method to force update timestamp (used by repository layer)
    public void ForceUpdateTimestamp()
    {
        SetUpdatedAt();
    }
}
