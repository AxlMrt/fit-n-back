using System.Text.RegularExpressions;

namespace FitnessApp.SharedKernel.ValueObjects;

/// <summary>
/// Value Object representing a username with validation rules.
/// </summary>
public sealed class Username : IEquatable<Username>
{
    private static readonly Regex UsernameRegex = new Regex(
        @"^[a-zA-Z0-9._-]{3,30}$", 
        RegexOptions.Compiled);

    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));

        username = username.Trim();

        if (username.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters long", nameof(username));

        if (username.Length > 30)
            throw new ArgumentException("Username cannot exceed 30 characters", nameof(username));

        if (!UsernameRegex.IsMatch(username))
            throw new ArgumentException("Username can only contain letters, numbers, dots, underscores and hyphens", nameof(username));

        // Additional business rules
        if (username.StartsWith(".") || username.EndsWith("."))
            throw new ArgumentException("Username cannot start or end with a dot", nameof(username));

        if (username.Contains(".."))
            throw new ArgumentException("Username cannot contain consecutive dots", nameof(username));

        return new Username(username);
    }

    public bool Equals(Username? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Username other && Equals(other);

    public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();

    public override string ToString() => Value;

    public static implicit operator string(Username username) => username.Value;

    public static bool operator ==(Username? left, Username? right) => Equals(left, right);

    public static bool operator !=(Username? left, Username? right) => !Equals(left, right);
}
