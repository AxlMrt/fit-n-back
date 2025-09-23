using FitnessApp.Modules.Tracking.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Domain.Entities;

/// <summary>
/// Aggregate root representing a workout session with actual tracking data
/// </summary>
public class WorkoutSession
{
    private readonly List<WorkoutSessionExercise> _exercises = [];

    private WorkoutSession() { } // For EF Core

    public WorkoutSession(
        Guid userId,
        Guid workoutId,
        DateTime? plannedDate = null,
        bool isFromProgram = false,
        Guid? programId = null)
    {
        if (userId == Guid.Empty)
            throw new TrackingDomainException("User ID cannot be empty");
            
        if (workoutId == Guid.Empty)
            throw new TrackingDomainException("Workout ID cannot be empty");

        Id = Guid.NewGuid();
        UserId = userId;
        WorkoutId = workoutId;
        PlannedDate = plannedDate;
        IsFromProgram = isFromProgram;
        ProgramId = programId;
        Status = plannedDate.HasValue ? WorkoutSessionStatus.Planned : WorkoutSessionStatus.InProgress;
        CreatedAt = DateTime.UtcNow;

        if (Status == WorkoutSessionStatus.InProgress)
        {
            StartTime = DateTime.UtcNow;
        }
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid WorkoutId { get; private set; }
    public DateTime? PlannedDate { get; private set; }
    public DateTime? StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public WorkoutSessionStatus Status { get; private set; }
    public int? TotalDurationSeconds { get; private set; }
    public int? CaloriesEstimated { get; private set; }
    public PerceivedDifficulty? PerceivedDifficulty { get; private set; }
    public string? Notes { get; private set; }
    public bool IsFromProgram { get; private set; }
    public Guid? ProgramId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public IReadOnlyList<WorkoutSessionExercise> Exercises => _exercises.AsReadOnly();

    // Computed properties
    public int ExerciseCount => _exercises.Count;
    public TimeSpan? Duration => StartTime.HasValue && EndTime.HasValue 
        ? EndTime.Value - StartTime.Value 
        : null;

    #region Session Management

    /// <summary>
    /// Start the workout session
    /// </summary>
    public void Start()
    {
        if (Status != WorkoutSessionStatus.Planned)
            throw new TrackingDomainException($"Cannot start session with status {Status}");

        Status = WorkoutSessionStatus.InProgress;
        StartTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Complete the workout session
    /// </summary>
    public void Complete(PerceivedDifficulty perceivedDifficulty, string? notes = null)
    {
        if (Status != WorkoutSessionStatus.InProgress)
            throw new TrackingDomainException($"Cannot complete session with status {Status}");

        if (!StartTime.HasValue)
            throw new TrackingDomainException("Cannot complete session without start time");

        Status = WorkoutSessionStatus.Completed;
        EndTime = DateTime.UtcNow;
        TotalDurationSeconds = (int)(EndTime.Value - StartTime.Value).TotalSeconds;
        PerceivedDifficulty = perceivedDifficulty;
        Notes = notes?.Trim();
        
        // Les calories seront calculées automatiquement 
        CaloriesEstimated = CalculateEstimatedCalories();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Calculate estimated calories burned based on duration and exercise types
    /// This is a basic estimation - in a real app, this would use user profile (weight, age, sex)
    /// and more sophisticated algorithms
    /// </summary>
    private int CalculateEstimatedCalories()
    {
        if (!TotalDurationSeconds.HasValue || TotalDurationSeconds.Value == 0)
            return 0;

        // Base metabolic rate per minute (rough estimation for average adult)
        var baseMETPerMinute = 5.0; // Moderate intensity workout
        var durationMinutes = TotalDurationSeconds.Value / 60.0;
        
        // Rough calorie calculation: MET * weight_kg * time_hours
        // Using average weight of 70kg for now - in real app, get from user profile
        var estimatedCalories = baseMETPerMinute * 70 * (durationMinutes / 60);

        return (int)Math.Round(estimatedCalories);
    }

    /// <summary>
    /// Abandon the workout session
    /// </summary>
    public void Abandon(string? reason = null)
    {
        if (Status != WorkoutSessionStatus.InProgress)
            throw new TrackingDomainException($"Cannot abandon session with status {Status}");

        Status = WorkoutSessionStatus.Abandoned;
        if (StartTime.HasValue)
        {
            EndTime = DateTime.UtcNow;
            TotalDurationSeconds = (int)(EndTime.Value - StartTime.Value).TotalSeconds;
        }
        Notes = reason?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancel a planned workout session
    /// </summary>
    public void Cancel(string? reason = null)
    {
        if (Status != WorkoutSessionStatus.Planned)
            throw new TrackingDomainException($"Cannot cancel session with status {Status}");

        Status = WorkoutSessionStatus.Cancelled;
        Notes = reason?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Exercise Management

    /// <summary>
    /// Add exercise performance to the session
    /// </summary>
    public WorkoutSessionExercise AddExercise(
        Guid exerciseId,
        string exerciseName,
        ExerciseMetricType metricType,
        double? value = null,
        int? repetitions = null,
        double? weight = null,
        int? durationSeconds = null,
        double? distance = null,
        int? sets = null)
    {
        if (Status != WorkoutSessionStatus.InProgress)
            throw new TrackingDomainException("Can only add exercises to sessions in progress");

        if (exerciseId == Guid.Empty)
            throw new TrackingDomainException("Exercise ID cannot be empty");

        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new TrackingDomainException("Exercise name is required");

        var exercise = new WorkoutSessionExercise(
            Id,
            exerciseId,
            exerciseName.Trim(),
            metricType,
            _exercises.Count + 1);

        // Add the initial set if any performance data is provided
        if (repetitions.HasValue || weight.HasValue || durationSeconds.HasValue || distance.HasValue)
        {
            exercise.AddSet(
                repetitions,
                weight,
                durationSeconds,
                distance);
        }

        _exercises.Add(exercise);
        UpdatedAt = DateTime.UtcNow;

        return exercise;
    }

    /// <summary>
    /// Remove exercise from the session
    /// </summary>
    public void RemoveExercise(Guid exerciseId)
    {
        if (Status != WorkoutSessionStatus.InProgress)
            throw new TrackingDomainException("Can only modify exercises in sessions in progress");

        var exercise = _exercises.FirstOrDefault(e => e.Id == exerciseId);
        if (exercise == null)
            throw new TrackingDomainException($"Exercise {exerciseId} not found in session");

        _exercises.Remove(exercise);
        UpdateExerciseOrders();
        UpdatedAt = DateTime.UtcNow;
    }

    private void UpdateExerciseOrders()
    {
        for (int i = 0; i < _exercises.Count; i++)
        {
            _exercises[i].UpdateOrder(i + 1);
        }
    }

    #endregion

    #region Update Methods

    public void UpdatePlannedDate(DateTime? plannedDate)
    {
        if (Status != WorkoutSessionStatus.Planned)
            throw new TrackingDomainException("Can only update planned date for planned sessions");

        PlannedDate = plannedDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetEstimatedCalories(int calories)
    {
        if (calories < 0)
            throw new TrackingDomainException("Calories cannot be negative");

        CaloriesEstimated = calories;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Query Methods

    public WorkoutSessionExercise? GetExercise(Guid exerciseId) =>
        _exercises.FirstOrDefault(e => e.Id == exerciseId);

    public bool HasExercise(Guid exerciseId) =>
        _exercises.Any(e => e.ExerciseId == exerciseId);

    public IEnumerable<WorkoutSessionExercise> GetExercisesByType(ExerciseMetricType metricType) =>
        _exercises.Where(e => e.MetricType == metricType);

    public bool IsOverdue() =>
        Status == WorkoutSessionStatus.Planned && 
        PlannedDate.HasValue && 
        PlannedDate.Value.Date < DateTime.UtcNow.Date;

    public bool IsUpcoming() =>
        Status == WorkoutSessionStatus.Planned && 
        PlannedDate.HasValue && 
        PlannedDate.Value.Date >= DateTime.UtcNow.Date;

    /// <summary>
    /// Calculate overall performance score for this workout (0-100)
    /// </summary>
    public double CalculatePerformanceScore()
    {
        if (Status != WorkoutSessionStatus.Completed || !_exercises.Any())
            return 0;

        var exerciseScores = _exercises.Select(e => e.GetOverallPerformanceScore()).ToList();
        var averageScore = exerciseScores.Average();

        // Bonus factors
        double bonus = 0;

        // Completion bonus
        if (_exercises.All(e => e.Sets.Any()))
            bonus += 5; // All exercises completed

        // Duration bonus (reasonable workout time)
        if (EndTime.HasValue && StartTime.HasValue)
        {
            var duration = (EndTime.Value - StartTime.Value).TotalMinutes;
            if (duration >= 20 && duration <= 120)
                bonus += 3; // Reasonable workout duration
        }

        // Consistency bonus (all exercises have similar performance)
        if (exerciseScores.Count > 1)
        {
            var scoreVariance = exerciseScores.Max() - exerciseScores.Min();
            if (scoreVariance <= 15) // Low variance = consistent performance
                bonus += 2;
        }

        return Math.Min(100, averageScore + bonus);
    }

    /// <summary>
    /// Get a summary of the workout performance
    /// </summary>
    public string GetPerformanceSummary()
    {
        if (Status != WorkoutSessionStatus.Completed)
            return "Workout not completed";

        var score = CalculatePerformanceScore();
        var description = Domain.Services.PerformanceAnalysisService.GetPerformanceDescription(score);
        var duration = (EndTime.HasValue && StartTime.HasValue) 
            ? $" ({(EndTime.Value - StartTime.Value).TotalMinutes:F0}min)" 
            : "";
        
        return $"{description} - {score:F0}%{duration}";
    }

    #endregion
}
