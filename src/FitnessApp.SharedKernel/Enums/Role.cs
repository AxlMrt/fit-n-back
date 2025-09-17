using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Represents the role of a user in the application
/// </summary>
[Flags]
public enum Role
{
    [Description("Athlete role")]
    Athlete = 1 << 0,

    [Description("Coach role")]
    Coach = 1 << 1,

    [Description("Admin role")]
    Admin = 1 << 2
}
