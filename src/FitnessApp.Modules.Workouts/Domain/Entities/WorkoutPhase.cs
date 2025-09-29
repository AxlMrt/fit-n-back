using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Domain.Entities;

/// <summary>
/// Represents a phase of a workout (warm-up, main effort, recovery, etc.)
/// </summary>
public class WorkoutPhase
{
    private readonly List<WorkoutExercise> _exercises = [];
    
    private WorkoutPhase() { }

    public WorkoutPhase(
        WorkoutPhaseType type, 
        string name, 
        int order)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw WorkoutDomainException.PhaseNameRequired();
            
        if (name.Length > 100)
            throw WorkoutDomainException.PhaseNameTooLong(100);
            
        if (order < 1)
            throw WorkoutDomainException.InvalidPhaseOrder();

        Id = Guid.NewGuid();
        Type = type;
        Name = name.Trim();
        EstimatedDurationMinutes = GetDefaultDurationForType(type);
        Order = order;
    }

    public Guid Id { get; private set; }
    public WorkoutPhaseType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int EstimatedDurationMinutes { get; private set; }
    public int Order { get; private set; }

    // Navigation properties
    public Guid WorkoutId { get; private set; }
    public Workout Workout { get; private set; } = null!;
    public IReadOnlyList<WorkoutExercise> Exercises => _exercises.AsReadOnly();

    // Computed properties
    public int ExerciseCount => _exercises.Count;

    #region Exercise Management

    // Signature principale avec tous les paramètres
    public void AddExercise(Guid exerciseId, int? sets = null, int? reps = null, 
        int? durationSeconds = null, double? distance = null, double? weight = null, int? restSeconds = null)
    {
        if (_exercises.Any(e => e.ExerciseId == exerciseId))
            throw WorkoutDomainException.ExerciseAlreadyExists(exerciseId);

        var nextOrder = _exercises.Count + 1;
        var workoutExercise = new WorkoutExercise(exerciseId, sets, reps, durationSeconds, 
            distance, weight, restSeconds, nextOrder);
        _exercises.Add(workoutExercise);
        RecalculateDuration(); // ✅ Recalcul automatique après ajout d'exercice
    }

    public void AddExercise(Guid exerciseId, int sets, int reps, int? restSeconds)
    {
        if (_exercises.Any(e => e.ExerciseId == exerciseId))
            throw WorkoutDomainException.ExerciseAlreadyExists(exerciseId);

        var nextOrder = _exercises.Count + 1;
        var workoutExercise = new WorkoutExercise(exerciseId, sets, reps, restSeconds, nextOrder);
        _exercises.Add(workoutExercise);
        RecalculateDuration();
    }

    public void AddRepBasedExercise(Guid exerciseId, int sets, int reps, int? restSeconds = null)
    {
        AddExercise(exerciseId, sets, reps, null, null, null, restSeconds);
    }

    public void AddTimeBasedExercise(Guid exerciseId, int durationSeconds, int? restSeconds = null)
    {
        AddExercise(exerciseId, null, null, durationSeconds, null, null, restSeconds);
    }

    public void AddDistanceBasedExercise(Guid exerciseId, double distanceMeters, int? restSeconds = null)
    {
        AddExercise(exerciseId, null, null, null, distanceMeters, null, restSeconds);
    }

    public void AddDistanceBasedExercise(Guid exerciseId, double distanceMeters)
    {
        AddExercise(exerciseId, null, null, null, distanceMeters, null, null);
    }

    public void AddWeightBasedExercise(Guid exerciseId, int sets, int reps, double weight, int? restSeconds = null)
    {
        AddExercise(exerciseId, sets, reps, null, null, weight, restSeconds);
    }

    public void RemoveExercise(Guid exerciseId)
    {
        var exercise = _exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
        if (exercise == null)
            throw WorkoutDomainException.ExerciseNotFound(exerciseId);

        _exercises.Remove(exercise);
        UpdateExerciseOrders();
        RecalculateDuration();
    }

    public void MoveExercise(Guid exerciseId, int newOrder)
    {
        var exercise = _exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
        if (exercise == null)
            throw WorkoutDomainException.ExerciseNotFound(exerciseId);

        if (newOrder < 1 || newOrder > _exercises.Count)
            throw WorkoutDomainException.InvalidExerciseOrder(_exercises.Count);

        var currentOrder = exercise.Order;
        
        // If no change needed, return
        if (currentOrder == newOrder)
            return;

        // Temporarily set the exercise order to be able to sort properly
        exercise.UpdateOrder(newOrder);
        
        // Adjust orders of other exercises
        foreach (var otherExercise in _exercises.Where(e => e != exercise))
        {
            if (currentOrder < newOrder)
            {
                // Moving down: shift exercises up
                if (otherExercise.Order > currentOrder && otherExercise.Order <= newOrder)
                {
                    otherExercise.UpdateOrder(otherExercise.Order - 1);
                }
            }
            else
            {
                // Moving up: shift exercises down
                if (otherExercise.Order >= newOrder && otherExercise.Order < currentOrder)
                {
                    otherExercise.UpdateOrder(otherExercise.Order + 1);
                }
            }
        }
    }

    private void UpdateExerciseOrders()
    {
        var orderedExercises = _exercises.OrderBy(e => e.Order).ToList();
        for (int i = 0; i < orderedExercises.Count; i++)
        {
            orderedExercises[i].UpdateOrder(i + 1);
        }
    }

    #endregion

    #region Update Methods

    /// <summary>
    /// Updates phase details with automatic duration recalculation
    /// </summary>
    public void UpdateDetails(string name, string? description = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            if (name.Length > 100)
                throw WorkoutDomainException.PhaseNameTooLong(100);
            Name = name.Trim();
        }

        Description = description?.Trim();
    }

    public void UpdateOrder(int newOrder)
    {
        if (newOrder < 1)
            throw WorkoutDomainException.InvalidPhaseOrder();
        Order = newOrder;
    }

    #endregion

    #region Business Methods

    public WorkoutExercise? GetExercise(Guid exerciseId) =>
        _exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);

    public bool HasExercise(Guid exerciseId) =>
        _exercises.Any(e => e.ExerciseId == exerciseId);

    /// <summary>
    /// Calculates the total estimated time for this phase based on exercises
    /// </summary>
    public int CalculateEstimatedTotalMinutes()
    {
        if (!_exercises.Any())
            return EstimatedDurationMinutes;

        var exerciseTime = _exercises.Sum(e => e.EstimateTimeMinutes());
        return Math.Max(EstimatedDurationMinutes, exerciseTime);
    }

    /// <summary>
    /// Automatically recalculates phase duration based on exercises
    /// </summary>
    public void RecalculateDuration()
    {
        if (_exercises.Any())
        {
            EstimatedDurationMinutes = _exercises.Sum(e => e.EstimateTimeMinutes());
        }
        else
        {
            EstimatedDurationMinutes = GetDefaultDurationForType(Type);
        }
    }

    /// <summary>
    /// Gets default duration in minutes for each phase type
    /// </summary>
    private static int GetDefaultDurationForType(WorkoutPhaseType type)
    {
        return type switch
        {
            WorkoutPhaseType.WarmUp => 8,
            WorkoutPhaseType.MainEffort => 25,
            WorkoutPhaseType.Recovery => 10,
            WorkoutPhaseType.CoolDown => 5,
            WorkoutPhaseType.Stretching => 10,
            _ => 15
        };
    }

    #endregion
}
