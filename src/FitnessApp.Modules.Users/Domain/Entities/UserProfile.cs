using FitnessApp.Modules.Users.Domain.ValueObjects;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Entities;

public class UserProfile
{
    public Guid UserId { get; private set; }

    public FullName Name { get; private set; } = FullName.Empty;
    public DateOfBirth? DateOfBirth { get; private set; }
    public Gender? Gender { get; private set; }
    public PhysicalMeasurements PhysicalMeasurements { get; private set; } = PhysicalMeasurements.Empty;
    
    public FitnessLevel? FitnessLevel { get; private set; }
    public FitnessGoal? FitnessGoal { get; private set; }

    public Subscription? Subscription { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private UserProfile() { }

    public UserProfile(Guid userId)
    {
        UserId = userId;
        CreatedAt = DateTime.UtcNow;
    }

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
        if (primaryGoal != null) FitnessGoal = primaryGoal;
        
        SetUpdatedAt();
    }

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

    // Profile Completion Logic
    public bool IsProfileComplete()
    {
        return Name != FullName.Empty
               && DateOfBirth != null
               && Gender != null
               && PhysicalMeasurements != PhysicalMeasurements.Empty
               && FitnessLevel != null
               && FitnessGoal != null;
    }

    public decimal GetProfileCompletionPercentage()
    {
        decimal totalFields = 6; // Name, DateOfBirth, Gender, PhysicalMeasurements, FitnessLevel, PrimaryGoal
        decimal completedFields = 0;

        if (Name != FullName.Empty) completedFields++;
        if (DateOfBirth != null) completedFields++;
        if (Gender != null) completedFields++;
        if (PhysicalMeasurements != PhysicalMeasurements.Empty) completedFields++;
        if (FitnessLevel != null) completedFields++;
        if (FitnessGoal != null) completedFields++;

        return Math.Round((completedFields / totalFields) * 100, 2);
    }

    // Calculated Properties
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
        if (PhysicalMeasurements.Height == null || PhysicalMeasurements.Weight == null)
            return null;

        var heightInMeters = PhysicalMeasurements.Height.Value / 100.0m;
        return PhysicalMeasurements.Weight.Value / (heightInMeters * heightInMeters);
    }

    // Business Logic Methods
    public bool HasCompletedProfile()
    {
        return IsProfileComplete();
    }

    public bool CanAccessPremiumFeatures()
    {
        return Subscription?.Level == SubscriptionLevel.Premium || 
               Subscription?.Level == SubscriptionLevel.Elite;
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
