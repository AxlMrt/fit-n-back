using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Defines the status of a workout session
/// </summary>
public enum WorkoutSessionStatus
{
    [Description("Workout is planned for future execution")]
    Planned = 0,

    [Description("Workout is currently in progress")]
    InProgress = 1,

    [Description("Workout has been completed successfully")]
    Completed = 2,

    [Description("Workout was started but not completed")]
    Abandoned = 3,

    [Description("Planned workout was cancelled")]
    Cancelled = 4
}

/// <summary>
/// Defines different types of user metrics that can be tracked
/// </summary>
public enum UserMetricType
{
    [Description("Body weight in kg")]
    Weight = 0,

    [Description("Height in cm")]
    Height = 1,

    [Description("Personal record for exercise performance")]
    PersonalRecord = 2
}

/// <summary>
/// Defines the perceived difficulty level of a completed workout
/// </summary>
public enum PerceivedDifficulty
{
    [Description("Very easy workout")]
    VeryEasy = 1,

    [Description("Easy workout")]
    Easy = 2,

    [Description("Moderate workout")]
    Moderate = 3,

    [Description("Hard workout")]
    Hard = 4,

    [Description("Very hard workout")]
    VeryHard = 5,

    [Description("Maximum effort")]
    Maximum = 6
}
