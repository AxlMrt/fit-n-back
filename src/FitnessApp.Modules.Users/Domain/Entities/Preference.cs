using FitnessApp.Modules.Users.Domain.Exceptions;

namespace FitnessApp.Modules.Users.Domain.Entities;

/// <summary>
/// Entity representing a user preference key-value pair.
/// </summary>
public class Preference
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public UserProfile UserProfile { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor
    private Preference() { }

    public Preference(Guid userId, string category, string key, string value)
    {
        if (userId == Guid.Empty)
            throw new UserDomainException("User ID is required");

        if (string.IsNullOrWhiteSpace(category))
            throw new UserDomainException("Preference category is required");

        if (string.IsNullOrWhiteSpace(key))
            throw new UserDomainException("Preference key is required");

        if (category.Length > 50)
            throw new UserDomainException("Preference category cannot exceed 50 characters");

        if (key.Length > 100)
            throw new UserDomainException("Preference key cannot exceed 100 characters");

        if (value?.Length > 1000)
            throw new UserDomainException("Preference value cannot exceed 1000 characters");

        Id = Guid.NewGuid();
        UserId = userId;
        Category = category.Trim();
        Key = key.Trim();
        Value = value?.Trim() ?? string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateValue(string value)
    {
        if (value?.Length > 1000)
            throw new UserDomainException("Preference value cannot exceed 1000 characters");

        Value = value?.Trim() ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    // Business query methods
    public bool IsNotificationPreference()
    {
        return Category.Equals("notifications", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsWorkoutPreference()
    {
        return Category.Equals("workout", StringComparison.OrdinalIgnoreCase);
    }

    public bool IsPrivacyPreference()
    {
        return Category.Equals("privacy", StringComparison.OrdinalIgnoreCase);
    }

    public T GetValueAs<T>()
    {
        try
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
        catch
        {
            throw new UserDomainException($"Cannot convert preference value '{Value}' to type {typeof(T).Name}");
        }
    }

    public bool GetBoolValue()
    {
        return bool.TryParse(Value, out var result) && result;
    }

    public int? GetIntValue()
    {
        return int.TryParse(Value, out var result) ? result : null;
    }
}