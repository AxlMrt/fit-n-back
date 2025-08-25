namespace FitnessApp.Modules.Workouts.Domain.Enums;

/// <summary>
/// Defines the different phases of a workout
/// </summary>
public enum WorkoutPhaseType
{
    /// <summary>
    /// Warm-up phase to prepare the body
    /// </summary>
    WarmUp = 1,
    
    /// <summary>
    /// Main effort phase containing the core exercises
    /// </summary>
    MainEffort = 2,
    
    /// <summary>
    /// Cool-down and recovery phase
    /// </summary>
    Recovery = 3,
    
    /// <summary>
    /// Stretching phase
    /// </summary>
    Stretching = 4
}
