namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Defines the valid categories for user preferences.
/// Each category groups related preference settings.
/// </summary>
public enum PreferenceCategory
{
    /// <summary>
    /// General application settings
    /// Keys: theme, language, timezone
    /// </summary>
    General,

    /// <summary>
    /// Notification preferences
    /// Keys: email, push, workout_reminders, goal_updates
    /// </summary>
    Notifications,

    /// <summary>
    /// Workout-related preferences
    /// Keys: default_duration, rest_time, difficulty_level, preferred_equipment
    /// </summary>
    Workout,

    /// <summary>
    /// Privacy and sharing settings
    /// Keys: profile_visibility, share_workouts, share_progress
    /// </summary>
    Privacy,

    /// <summary>
    /// Unit measurement preferences
    /// Keys: weight_unit (kg/lbs), distance_unit (m/miles), temperature_unit (c/f)
    /// </summary>
    Units,

    /// <summary>
    /// Goal tracking preferences
    /// Keys: weekly_goal, monthly_goal, reminder_frequency
    /// </summary>
    Goals,

    /// <summary>
    /// Display and UI preferences
    /// Keys: dashboard_layout, chart_type, date_format
    /// </summary>
    Display
}
