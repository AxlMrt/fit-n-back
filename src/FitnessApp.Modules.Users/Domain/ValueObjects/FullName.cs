using FitnessApp.Modules.Users.Domain.Exceptions;

namespace FitnessApp.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value Object representing a person's full name with validation.
/// </summary>
public sealed class FullName : IEquatable<FullName>
{
    public string? FirstName { get; }
    public string? LastName { get; }
    public string DisplayName { get; }

    private FullName(string? firstName, string? lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        DisplayName = GenerateDisplayName();
    }

    public static FullName Create(string? firstName = null, string? lastName = null)
    {
        firstName = ValidateAndNormalizeName(firstName, nameof(firstName));
        lastName = ValidateAndNormalizeName(lastName, nameof(lastName));

        return new FullName(firstName, lastName);
    }

    public static FullName Empty => new FullName(null, null);

    private static string? ValidateAndNormalizeName(string? name, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        name = name.Trim();

        if (name.Length > 50)
            throw UserDomainException.NameTooLong(parameterName);

        // Check for invalid characters (basic security check)
        if (name.Contains('<') || name.Contains('>') || name.Contains('&'))
            throw UserDomainException.NameContainsInvalidCharacters(parameterName);

        return name;
    }

    private string GenerateDisplayName()
    {
        if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
            return $"{FirstName} {LastName}";

        if (!string.IsNullOrEmpty(FirstName))
            return FirstName;

        if (!string.IsNullOrEmpty(LastName))
            return LastName;

        return "User";
    }

    public FullName UpdateFirstName(string? firstName)
    {
        return Create(firstName, LastName);
    }

    public FullName UpdateLastName(string? lastName)
    {
        return Create(FirstName, lastName);
    }

    public bool IsComplete => !string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName);

    public bool Equals(FullName? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(FirstName, other.FirstName, StringComparison.OrdinalIgnoreCase) && 
               string.Equals(LastName, other.LastName, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FullName other && Equals(other);

    public override int GetHashCode()
    {
        return HashCode.Combine(
            FirstName?.ToLowerInvariant(), 
            LastName?.ToLowerInvariant());
    }

    public override string ToString() => DisplayName;

    public static implicit operator string(FullName fullName) => fullName.DisplayName;

    public static bool operator ==(FullName? left, FullName? right) => Equals(left, right);

    public static bool operator !=(FullName? left, FullName? right) => !Equals(left, right);
}
