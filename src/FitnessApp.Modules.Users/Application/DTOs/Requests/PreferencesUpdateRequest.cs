namespace FitnessApp.Modules.Users.Application.DTOs.Requests;

public record PreferenceItem(string Category, string Key, string Value);

public record PreferencesUpdateRequest(IReadOnlyCollection<PreferenceItem> Items);
