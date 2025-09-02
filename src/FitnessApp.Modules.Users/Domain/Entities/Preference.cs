using FitnessApp.Modules.Users.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Entities;

/// <summary>
/// Entity representing a user preference key-value pair.
/// </summary>
public class Preference
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public PreferenceCategory Category { get; private set; }
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // EF Core constructor
    private Preference() { }

    public Preference(Guid userId, PreferenceCategory category, string key, string value)
    {
        if (userId == Guid.Empty)
            throw new UserDomainException("User ID is required");

        if (string.IsNullOrWhiteSpace(key))
            throw new UserDomainException("Preference key is required");

        if (key.Length > 100)
            throw new UserDomainException("Preference key cannot exceed 100 characters");

        if (value?.Length > 1000)
            throw new UserDomainException("Preference value cannot exceed 1000 characters");

        Id = Guid.NewGuid();
        UserId = userId;
        Category = category;
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
        return Category == PreferenceCategory.Notifications;
    }

    public bool IsWorkoutPreference()
    {
        return Category == PreferenceCategory.Workout;
    }

    public bool IsPrivacyPreference()
    {
        return Category == PreferenceCategory.Privacy;
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