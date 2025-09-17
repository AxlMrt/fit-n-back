using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Represents the fitness level of a user.
/// </summary>
[Flags]
public enum FitnessLevel
{
    [Description("Beginner")]
    Beginner = 1 << 0,

    [Description("Enthousiast")]
    Enthousiast = 1 << 1,

    [Description("Advanced")]
    Advanced = 1 << 2,

    [Description("Athlete")]
    Athlete = 1 << 3
}