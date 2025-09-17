using System.ComponentModel;

namespace FitnessApp.SharedKernel.Enums;

/// <summary>
/// Represents the subscription level of a user
/// </summary>
[Flags]
public enum SubscriptionLevel
{
    [Description("Free subscription")]
    Free = 1 << 0,

    [Description("Basic subscription")]
    Basic = 1 << 1,

    [Description("Premium subscription")]
    Premium = 1 << 2,

    [Description("Elite subscription")]
    Elite = 1 << 3
}
