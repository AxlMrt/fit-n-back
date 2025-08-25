namespace FitnessApp.Modules.Workouts.Domain.Exceptions;

/// <summary>
/// Base exception for Workout domain errors
/// </summary>
public class WorkoutDomainException : Exception
{
    public WorkoutDomainException(string message) : base(message)
    {
    }

    public WorkoutDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
