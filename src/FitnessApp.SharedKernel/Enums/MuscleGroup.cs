using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Represents muscle groups targeted by exercises
/// </summary>
[Flags]
public enum MuscleGroup
{
    [Description("No muscle group")]
    None = 0,

    [Description("Chest")]
    Chest = 1 << 0,

    [Description("Back")]
    Back = 1 << 1,

    [Description("Legs")]
    Legs = 1 << 2,

    [Description("Glutes")]
    Glutes = 1 << 3,

    [Description("Shoulders")]
    Shoulders = 1 << 4,

    [Description("Arms")]
    Arms = 1 << 5,

    [Description("Triceps")]
    Triceps = 1 << 6,

    [Description("Core")]
    Core = 1 << 7,

    [Description("Full body")]
    Full_Body = 1 << 8
}
