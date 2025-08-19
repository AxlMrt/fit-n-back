namespace FitnessApp.Modules.Exercises.Domain.Enums
{
    [Flags]
    public enum MuscleGroup
    {
        NONE = 0,
        CHEST = 1 << 0,
        BACK = 1 << 1,
        LEGS = 1 << 2,
        SHOULDERS = 1 << 3,
        ARMS = 1 << 4,
        CORE = 1 << 5,
        FULL_BODY = 1 << 6
    }
}
