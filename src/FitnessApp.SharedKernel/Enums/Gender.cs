using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Represents the gender of a user.
/// </summary>
[Flags]
public enum Gender
{
    [Description("Male")]
    Male = 1 << 0,

    [Description("Female")]
    Female = 1 << 1,

    [Description("Other")]
    Other = 1 << 2,

    [Description("Prefer not to say")]
    PreferNotToSay = 1 << 3
}
