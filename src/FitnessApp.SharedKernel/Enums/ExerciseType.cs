using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Represents the type of exercise
/// </summary>
public enum ExerciseType
{
    [Description("Strength exercise")]
    Strength = 1 << 0,

    [Description("Cardio exercise")]
    Cardio = 1 << 1,

    [Description("Mobility exercise")]
    Mobility = 1 << 2,

    [Description("Stretching exercise")]
    Stretching = 1 << 3,

    [Description("Plyometrics exercise")]
    Plyometrics = 1 << 4,

    [Description("HIIT (High Intensity Interval Training)")]
    HIIT = 1 << 5,

    [Description("Warmup exercise")]
    Warmup = 1 << 6,

    [Description("Other type")]
    Other = 1 << 7
}
