using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Domain.Services;

public class UserPreferenceDomainService
{
    public Preference AddOrUpdatePreference(Guid userId, PreferenceCategory category, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw UserDomainException.PreferenceKeyRequired();

        return new Preference(userId, category, key, value ?? string.Empty);
    }

    public bool CanRemovePreference(PreferenceCategory category, string key)
    {
        return !IsRequiredPreference(category, key);
    }

    private bool IsRequiredPreference(PreferenceCategory category, string key)
    {
        return category == PreferenceCategory.Privacy && key == "data_processing_consent";
    }

    public bool IsValidPreferenceValue(PreferenceCategory category, string key, string value)
    {
        return category switch
        {
            PreferenceCategory.General when key == "theme" => value is "light" or "dark" or "auto",
            PreferenceCategory.General when key == "language" => IsValidLanguageCode(value),
            PreferenceCategory.Units when key == "height_unit" => value is "cm" or "ft" or "in",
            PreferenceCategory.Units when key == "weight_unit" => value is "kg" or "lbs" or "lb",
            PreferenceCategory.Units when key == "locale" => IsValidLanguageCode(value) || value.Contains("-"),
            PreferenceCategory.Notifications => IsBooleanString(value),
            PreferenceCategory.Privacy when key == "profile_visibility" => value is "public" or "friends" or "private",
            _ => !string.IsNullOrWhiteSpace(value)
        };
    }

    private bool IsValidLanguageCode(string value)
    {
        // Codes ISO 639-1 standards
        var validLanguages = new[] { "en", "fr", "es", "de", "it", "pt", "nl" };
        return validLanguages.Contains(value?.ToLower());
    }

    private bool IsBooleanString(string value)
    {
        return value is "true" or "false";
    }
}
