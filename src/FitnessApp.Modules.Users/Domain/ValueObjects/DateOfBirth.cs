using FitnessApp.Modules.Users.Domain.Exceptions;

namespace FitnessApp.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value Object representing a date of birth with age calculation.
/// </summary>
public sealed class DateOfBirth : IEquatable<DateOfBirth>
{
    public DateTime Value { get; }
    public int Age { get; }

    private DateOfBirth(DateTime value)
    {
        // Ensure the DateTime is stored as UTC with only the date part
        Value = DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
        Age = CalculateAge();
    }

    public static DateOfBirth Create(DateTime dateOfBirth)
    {
        // Normalize to date only and ensure UTC kind
        dateOfBirth = DateTime.SpecifyKind(dateOfBirth.Date, DateTimeKind.Utc);

        if (dateOfBirth > DateTime.UtcNow.Date)
            throw new UserDomainException("Date of birth cannot be in the future");

        var age = CalculateAgeFromDate(dateOfBirth);
        
        if (age > 120)
            throw new UserDomainException("Date of birth indicates an unrealistic age");

        if (age < 13)
            throw new UserDomainException("Users must be at least 13 years old");

        return new DateOfBirth(dateOfBirth);
    }

    private int CalculateAge()
    {
        return CalculateAgeFromDate(Value);
    }

    private static int CalculateAgeFromDate(DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;
        
        if (dateOfBirth.Date > today.AddYears(-age))
            age--;

        return age;
    }

    public bool IsAdult => Age >= 18;
    
    public bool IsSenior => Age >= 65;

    public string GetAgeGroup()
    {
        return Age switch
        {
            < 18 => "Teen",
            >= 18 and < 30 => "Young Adult",
            >= 30 and < 50 => "Adult",
            >= 50 and < 65 => "Middle-aged",
            >= 65 => "Senior"
        };
    }

    public bool Equals(DateOfBirth? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value);
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is DateOfBirth other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => $"{Value:yyyy-MM-dd} (Age: {Age})";

    public static implicit operator DateTime(DateOfBirth dateOfBirth) => dateOfBirth.Value;

    public static bool operator ==(DateOfBirth? left, DateOfBirth? right) => Equals(left, right);

    public static bool operator !=(DateOfBirth? left, DateOfBirth? right) => !Equals(left, right);
}
