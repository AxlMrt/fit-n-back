using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Defines the difficulty level of a workout
/// </summary>
[Flags]
public enum DifficultyLevel
{
    [Description("Beginner level")]
    Beginner = 1 << 0,

    [Description("Intermediate level")]
    Intermediate = 1 << 1,

    [Description("Advanced level")]
    Advanced = 1 << 2,

    [Description("Expert level")]
    Expert = 1 << 3
}
