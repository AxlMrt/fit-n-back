namespace FitnessApp.Modules.Workouts.Domain.Enums;

/// <summary>
/// Defines the type of workout based on how it was created
/// </summary>
public enum WorkoutType
{
    /// <summary>
    /// Dynamically generated workout based on user profile, objectives, and level
    /// </summary>
    Dynamic = 1,
    
    /// <summary>
    /// Pre-configured workout created by the system or coaches
    /// </summary>
    Fixed = 2,
    
    /// <summary>
    /// Custom workout created by the user
    /// </summary>
    UserCreated = 3
}
