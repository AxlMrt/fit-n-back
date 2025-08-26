using FitnessApp.Modules.Authentication.Domain.Exceptions;

namespace FitnessApp.Modules.Authentication.Domain.ValueObjects;

/// <summary>
/// Value object representing a securely hashed password with verification capabilities.
/// </summary>
public sealed class PasswordHash : IEquatable<PasswordHash>
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static PasswordHash Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new AuthenticationDomainException("Password cannot be empty");

        if (password.Length < 8)
            throw new AuthenticationDomainException("Password must be at least 8 characters long");

        if (password.Length > 128)
            throw new AuthenticationDomainException("Password cannot exceed 128 characters");

        // Validate password complexity
        ValidatePasswordComplexity(password);

        // Hash the password using BCrypt
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        return new PasswordHash(hash);
    }

    public static PasswordHash FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new AuthenticationDomainException("Password hash cannot be empty");

        return new PasswordHash(hash);
    }

    public bool Verify(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, Value);
        }
        catch
        {
            return false;
        }
    }

    private static void ValidatePasswordComplexity(string password)
    {
        bool hasUpper = false;
        bool hasLower = false;
        bool hasDigit = false;
        bool hasSpecial = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else if (IsSpecialCharacter(c)) hasSpecial = true;
        }

        var missingRequirements = new List<string>();
        if (!hasUpper) missingRequirements.Add("uppercase letter");
        if (!hasLower) missingRequirements.Add("lowercase letter");
        if (!hasDigit) missingRequirements.Add("digit");
        if (!hasSpecial) missingRequirements.Add("special character");

        if (missingRequirements.Any())
        {
            throw new AuthenticationDomainException(
                $"Password must contain at least one: {string.Join(", ", missingRequirements)}");
        }
    }

    private static bool IsSpecialCharacter(char c)
    {
        return "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c);
    }

    public bool Equals(PasswordHash? other)
    {
        return other is not null && Value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PasswordHash);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(PasswordHash? left, PasswordHash? right)
    {
        return EqualityComparer<PasswordHash>.Default.Equals(left, right);
    }

    public static bool operator !=(PasswordHash? left, PasswordHash? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"PasswordHash: {Value[..8]}***";
    }
}
