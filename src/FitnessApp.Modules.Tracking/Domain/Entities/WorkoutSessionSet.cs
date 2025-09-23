using FitnessApp.Modules.Tracking.Domain.Exceptions;

namespace FitnessApp.Modules.Tracking.Domain.Entities;

/// <summary>
/// Represents a single set within an exercise during a workout session
/// Each set captures the specific performance metrics for that attempt
/// </summary>
public class WorkoutSessionSet
{
    /// <summary>
    /// Unique identifier for this set
    /// </summary>
    public Guid Id { get; private set; }
    /// <summary>
    /// The workout session exercise this set belongs to
    /// </summary>
    public Guid WorkoutSessionExerciseId { get; private set; }

    /// <summary>
    /// Navigation property to the parent exercise
    /// </summary>
    public WorkoutSessionExercise WorkoutSessionExercise { get; private set; } = null!;

    /// <summary>
    /// Order of this set within the exercise (1st set, 2nd set, etc.)
    /// </summary>
    public int SetNumber { get; private set; }

    /// <summary>
    /// Number of repetitions performed in this set (for rep-based exercises)
    /// </summary>
    public int? Repetitions { get; private set; }

    /// <summary>
    /// Weight used in this set (in kg)
    /// </summary>
    public double? Weight { get; private set; }

    /// <summary>
    /// Duration of this set in seconds (for time-based exercises)
    /// </summary>
    public int? DurationSeconds { get; private set; }

    /// <summary>
    /// Distance covered in this set (for distance-based exercises)
    /// </summary>
    public double? Distance { get; private set; }

    /// <summary>
    /// When this set was completed
    /// </summary>
    public DateTime CompletedAt { get; private set; }

    /// <summary>
    /// Optional rest time after this set (in seconds)
    /// </summary>
    public int? RestTimeSeconds { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private WorkoutSessionSet() { }

    /// <summary>
    /// Create a new workout session set
    /// </summary>
    public WorkoutSessionSet(
        Guid workoutSessionExerciseId,
        int setNumber,
        int? repetitions = null,
        double? weight = null,
        int? durationSeconds = null,
        double? distance = null,
        int? restTimeSeconds = null)
    {
        Id = Guid.NewGuid();
        WorkoutSessionExerciseId = workoutSessionExerciseId;
        SetNumber = setNumber;
        Repetitions = repetitions;
        Weight = weight;
        DurationSeconds = durationSeconds;
        Distance = distance;
        RestTimeSeconds = restTimeSeconds;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update the performance metrics for this set
    /// </summary>
    public void UpdatePerformance(
        int? repetitions = null,
        double? weight = null,
        int? durationSeconds = null,
        double? distance = null,
        int? restTimeSeconds = null)
    {
        if (repetitions.HasValue)
            Repetitions = repetitions.Value;
        
        if (weight.HasValue)
            Weight = weight.Value;
        
        if (durationSeconds.HasValue)
            DurationSeconds = durationSeconds.Value;
        
        if (distance.HasValue)
            Distance = distance.Value;
        
        if (restTimeSeconds.HasValue)
            RestTimeSeconds = restTimeSeconds.Value;
    }

    /// <summary>
    /// Mark this set as completed with current timestamp
    /// </summary>
    public void MarkCompleted()
    {
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Get the primary performance value based on what metrics are available
    /// Priority: Weight > Distance > DurationSeconds > Repetitions
    /// </summary>
    public double? GetPrimaryPerformanceValue()
    {
        if (Weight.HasValue && Repetitions.HasValue)
            return Weight.Value * Repetitions.Value; // Volume pour musculation

        if (Distance.HasValue)
            return Distance.Value; // Distance pour cardio

        if (DurationSeconds.HasValue)
            return DurationSeconds.Value; // Durée pour exercices isométriques

        if (Repetitions.HasValue)
            return Repetitions.Value; // Répétitions pour poids du corps

        return null;
    }

    /// <summary>
    /// Get a performance score for this set (0-100)
    /// This is a basic implementation - use PerformanceAnalysisService for advanced scoring
    /// </summary>
    public double GetPerformanceScore()
    {
        var primaryValue = GetPrimaryPerformanceValue();
        if (!primaryValue.HasValue) return 0;

        // Base score for completing the set
        double baseScore = 75.0;

        // Bonus for good form indicators (could be expanded with more data)
        double formBonus = 0.0;
        
        // If rest time is reasonable (not too short, not too long), give bonus
        if (RestTimeSeconds.HasValue)
        {
            var restMinutes = RestTimeSeconds.Value / 60.0;
            if (restMinutes >= 0.5 && restMinutes <= 5.0)
                formBonus += 5.0; // Good rest timing
        }

        // Bonus for compound movements (weight + reps)
        if (Weight.HasValue && Repetitions.HasValue)
            formBonus += 5.0;

        return Math.Min(100.0, baseScore + formBonus);
    }

    /// <summary>
    /// Get display text for this set's performance
    /// </summary>
    public string GetDisplayText()
    {
        var parts = new List<string>();

        if (Repetitions.HasValue && Weight.HasValue)
            parts.Add($"{Repetitions}x{Weight:F1}kg");
        else if (Repetitions.HasValue)
            parts.Add($"{Repetitions} reps");

        if (DurationSeconds.HasValue)
        {
            var minutes = DurationSeconds.Value / 60;
            var seconds = DurationSeconds.Value % 60;
            parts.Add(seconds > 0 ? $"{minutes}:{seconds:D2}" : $"{minutes}min");
        }

        if (Distance.HasValue)
            parts.Add(Distance.Value >= 1000 ? $"{Distance.Value/1000:F1}km" : $"{Distance.Value:F0}m");

        return parts.Any() ? string.Join(" | ", parts) : "Completed";
    }
}
