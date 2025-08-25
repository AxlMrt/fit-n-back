using FitnessApp.Modules.Workouts.Domain.Enums;
using FitnessApp.Modules.Workouts.Domain.ValueObjects;
using FitnessApp.Modules.Workouts.Domain.Exceptions;

namespace FitnessApp.Modules.Workouts.Domain.Entities;

/// <summary>
/// Represents a phase of a workout (warm-up, main effort, recovery, etc.)
/// </summary>
public class WorkoutPhase
{
    private readonly List<WorkoutExercise> _exercises = [];
    
    private WorkoutPhase() { } // For EF Core

    public WorkoutPhase(
        WorkoutPhaseType type, 
        string name, 
        Duration estimatedDuration, 
        int order)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new WorkoutDomainException("Phase name is required");
            
        if (order < 0)
            throw new WorkoutDomainException("Order cannot be negative");

        Id = Guid.NewGuid();
        Type = type;
        Name = name.Trim();
        EstimatedDuration = estimatedDuration ?? throw new ArgumentNullException(nameof(estimatedDuration));
        Order = order;
    }

    public Guid Id { get; private set; }
    public WorkoutPhaseType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Duration EstimatedDuration { get; private set; } = null!;
    public int Order { get; private set; }

    // Navigation properties
    public Guid WorkoutId { get; private set; }
    public Workout Workout { get; private set; } = null!;
    public IReadOnlyList<WorkoutExercise> Exercises => _exercises.AsReadOnly();

    // Business methods
    public void SetDescription(string? description)
    {
        Description = description?.Trim();
    }

    public void UpdateEstimatedDuration(Duration duration)
    {
        EstimatedDuration = duration ?? throw new ArgumentNullException(nameof(duration));
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new WorkoutDomainException("Order cannot be negative");
            
        Order = order;
    }

    public WorkoutExercise AddExercise(
        Guid exerciseId, 
        string exerciseName, 
        ExerciseParameters parameters)
    {
        var nextOrder = _exercises.Count > 0 ? _exercises.Max(e => e.Order) + 1 : 0;
        var workoutExercise = new WorkoutExercise(exerciseId, exerciseName, parameters, nextOrder);
        _exercises.Add(workoutExercise);
        
        return workoutExercise;
    }

    public void RemoveExercise(Guid workoutExerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == workoutExerciseId);
        if (exercise == null)
            throw new WorkoutDomainException($"Exercise with ID {workoutExerciseId} not found in this phase");

        _exercises.Remove(exercise);
        ReorderExercises();
    }

    public void MoveExercise(Guid workoutExerciseId, int newOrder)
    {
        var exercise = _exercises.FirstOrDefault(e => e.Id == workoutExerciseId);
        if (exercise == null)
            throw new WorkoutDomainException($"Exercise with ID {workoutExerciseId} not found in this phase");

        if (newOrder < 0 || newOrder >= _exercises.Count)
            throw new WorkoutDomainException("Invalid exercise order position");

        _exercises.Remove(exercise);
        _exercises.Insert(newOrder, exercise);
        ReorderExercises();
    }

    private void ReorderExercises()
    {
        for (int i = 0; i < _exercises.Count; i++)
        {
            _exercises[i].UpdateOrder(i);
        }
    }

    public Duration CalculateTotalDuration()
    {
        if (!_exercises.Any())
            return EstimatedDuration;

        var totalDuration = TimeSpan.Zero;
        
        foreach (var exercise in _exercises)
        {
            if (exercise.Parameters.Duration.HasValue)
            {
                var sets = exercise.Parameters.Sets ?? 1;
                totalDuration += exercise.Parameters.Duration.Value * sets;
            }
            
            if (exercise.Parameters.RestTime.HasValue)
            {
                var sets = exercise.Parameters.Sets ?? 1;
                totalDuration += exercise.Parameters.RestTime.Value * (sets - 1); // Rest between sets
            }
        }

        return totalDuration > TimeSpan.Zero ? new Duration(totalDuration) : EstimatedDuration;
    }
}
