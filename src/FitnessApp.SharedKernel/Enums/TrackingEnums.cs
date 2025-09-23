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

    [Description("Body fat percentage")]
    BodyFatPercentage = 1,

    [Description("Muscle mass in kg")]
    MuscleMass = 2,

    [Description("Personal record for a specific exercise")]
    PersonalRecord = 3,

    [Description("Resting heart rate")]
    RestingHeartRate = 4,

    [Description("Blood pressure")]
    BloodPressure = 5,

    [Description("Body measurements (waist, chest, etc.)")]
    BodyMeasurement = 6,

    [Description("VO2 Max measurement")]
    VO2Max = 7,

    [Description("Other custom metric")]
    Other = 99
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
