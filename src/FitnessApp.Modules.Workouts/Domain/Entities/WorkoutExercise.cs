using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Domain.Entities;

/// <summary>
/// Represents an exercise within a workout phase with specific parameters
/// </summary>
public class WorkoutExercise
{
    private WorkoutExercise() { } // For EF Core

    public WorkoutExercise(
        Guid exerciseId, 
        string exerciseName,
        ExerciseParameters parameters, 
        int order)
    {
        if (exerciseId == Guid.Empty)
            throw new WorkoutDomainException("Exercise ID cannot be empty");
            
        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new WorkoutDomainException("Exercise name is required");
            
        if (order < 0)
            throw new WorkoutDomainException("Order cannot be negative");

        Id = Guid.NewGuid();
        ExerciseId = exerciseId;
        ExerciseName = exerciseName.Trim();
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        Order = order;
    }

    public Guid Id { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string ExerciseName { get; private set; } = string.Empty;
    public ExerciseParameters Parameters { get; private set; } = new();
    public int Order { get; private set; }

    // Navigation properties
    public Guid WorkoutPhaseId { get; private set; }
    public WorkoutPhase WorkoutPhase { get; private set; } = null!;

    // Business methods
    public void UpdateParameters(ExerciseParameters parameters)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new WorkoutDomainException("Order cannot be negative");
            
        Order = order;
    }

    public void UpdateExerciseName(string exerciseName)
    {
        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new WorkoutDomainException("Exercise name is required");
            
        ExerciseName = exerciseName.Trim();
    }
}
