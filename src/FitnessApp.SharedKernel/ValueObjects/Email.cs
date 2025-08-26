using System.Net.Mail;
using System.Text.RegularExpressions;

namespace FitnessApp.SharedKernel.ValueObjects;

/// <summary>
/// Value Object representing an email address with validation.
/// </summary>
public sealed class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new Regex(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        email = email.Trim().ToLowerInvariant();

        if (email.Length > 320) // RFC 5321 limit
            throw new ArgumentException("Email address is too long", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        // Additional validation using .NET MailAddress
        try
        {
            var mailAddress = new MailAddress(email);
            return new Email(mailAddress.Address);
        }
        catch
        {
            throw new ArgumentException("Invalid email format", nameof(email));
        }
    }

    public bool Equals(Email? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is Email other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;

    public static bool operator ==(Email? left, Email? right) => Equals(left, right);

    public static bool operator !=(Email? left, Email? right) => !Equals(left, right);
}
