using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Represents the user's main fitness goal
/// </summary>
[Flags]
public enum FitnessGoal
{
    [Description("Weight loss")]
    Weight_Loss = 1 << 0,

    [Description("Muscle gain")]
    Muscle_Gain = 1 << 1,

    [Description("Strength")]
    Strength = 1 << 2,

    [Description("Endurance")]
    Endurance = 1 << 3,

    [Description("Flexibility")]
    Flexibility = 1 << 4,

    [Description("Wellness")]
    Wellness = 1 << 5,

    [Description("Custom goal")]
    Custom = 1 << 6
}