using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Defines the valid categories for user preferences. Each category groups related preference settings.
/// </summary>
[Flags]
public enum PreferenceCategory
{
    [Description("General application settings")]
    General = 1 << 0,

    [Description("Notification preferences")]
    Notifications = 1 << 1,

    [Description("Workout-related preferences")]
    Workout = 1 << 2,

    [Description("Privacy and sharing settings")]
    Privacy = 1 << 3,

    [Description("Unit measurement preferences")]
    Units = 1 << 4,

    [Description("Goal tracking preferences")]
    Goals = 1 << 5
}
