namespace FitnessApp.Modules.Exercises.Domain.Enums;

[Flags]
public enum MuscleGroup
{
    NONE = 0,
    CHEST = 1 << 0,
    BACK = 1 << 1,
    LEGS = 1 << 2,
    GLUTES = 1 << 3,
    SHOULDERS = 1 << 4,
    ARMS = 1 << 5,
    TRICEPS = 1 << 6,
    CORE = 1 << 7,
    FULL_BODY = 1 << 8
}
