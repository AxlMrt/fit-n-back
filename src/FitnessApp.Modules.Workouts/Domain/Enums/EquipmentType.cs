using System.ComponentModel;

namespace FitnessApp.Modules.Workouts.Domain.Enums;

/// <summary>
/// Equipment required for workout
/// </summary>
[Flags]
public enum EquipmentType
{
    [Description("No equipment needed")]
    None = 0,
    
    [Description("Free weights (dumbbells, barbells)")]
    FreeWeights = 1 << 0,
    
    [Description("Resistance bands")]
    ResistanceBands = 1 << 1,
    
    [Description("Pull-up bar")]
    PullUpBar = 1 << 2,
    
    [Description("Exercise mat")]
    Mat = 1 << 3,
    
    [Description("Kettlebell")]
    Kettlebell = 1 << 4,
    
    [Description("Cable machine")]
    CableMachine = 1 << 5,
    
    [Description("Gym equipment")]
    GymEquipment = 1 << 6,
    
    [Description("Cardio equipment")]
    CardioEquipment = 1 << 7
}
