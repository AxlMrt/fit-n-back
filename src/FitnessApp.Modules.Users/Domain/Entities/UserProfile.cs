namespace FitnessApp.Modules.Users.Domain.Entities;

public class UserProfile
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Gender { get; private set; }

    public float? Height { get; private set; }
    public float? Weight { get; private set; }

    public string? FitnessLevel { get; private set; }
    public string? FitnessGoal { get; private set; }

    private UserProfile() { }

    public UserProfile(Guid userId)
    {
        UserId = userId;
    }

    public void UpdatePersonalInfo(string? firstName, string? lastName, DateTime? dateOfBirth, string? gender)
    {
        FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
        DateOfBirth = DateOfBirth = dateOfBirth?.ToUniversalTime();;
        Gender = string.IsNullOrWhiteSpace(gender) ? null : gender.Trim();
    }

    public void UpdatePhysicalInfo(float? height, float? weight)
    {
        if (height is > 50 and < 300)
            Height = height;

        if (weight is > 20 and < 500)
            Weight = weight;
    }

    public void UpdateFitnessInfo(string? fitnessLevel, string? fitnessGoal)
    {
        FitnessLevel = string.IsNullOrWhiteSpace(fitnessLevel) ? null : fitnessLevel.Trim();
        FitnessGoal = string.IsNullOrWhiteSpace(fitnessGoal) ? null : fitnessGoal.Trim();
    }

    public int? CalculateAge()
    {
        if (DateOfBirth == null)
            return null;

        var today = DateTime.Today;
        var age = today.Year - DateOfBirth.Value.Year;
        if (DateOfBirth.Value.Date > today.AddYears(-age))
            age--;

        return age;
    }

    public float? CalculateBMI()
    {
        if (Height is null or <= 0 || Weight is null or <= 0)
            return null;

        var heightInMeters = Height.Value / 100f;
        return (float)Math.Round(Weight.Value / (heightInMeters * heightInMeters), 2);
    }

    public string GetFullName()
    {
        if (!string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
            return $"{FirstName} {LastName}";

        return "Utilisateur";
    }

    public bool HasCompletedProfile()
    {
        return
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            DateOfBirth != null &&
            !string.IsNullOrWhiteSpace(Gender) &&
            Height != null &&
            Weight != null &&
            !string.IsNullOrWhiteSpace(FitnessLevel) &&
            !string.IsNullOrWhiteSpace(FitnessGoal);
    }
}
