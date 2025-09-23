using FitnessApp.Modules.Tracking.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Domain.Entities;

/// <summary>
/// Represents an exercise performed within a workout session with actual performance data
/// </summary>
public class WorkoutSessionExercise
{
    private WorkoutSessionExercise() { } // For EF Core

    public WorkoutSessionExercise(
        Guid workoutSessionId,
        Guid exerciseId,
        string exerciseName,
        ExerciseMetricType metricType,
        int order)
    {
        if (workoutSessionId == Guid.Empty)
            throw new TrackingDomainException("Workout session ID cannot be empty");
            
        if (exerciseId == Guid.Empty)
            throw new TrackingDomainException("Exercise ID cannot be empty");

        if (string.IsNullOrWhiteSpace(exerciseName))
            throw new TrackingDomainException("Exercise name is required");

        if (order < 1)
            throw new TrackingDomainException("Order must be at least 1");

        Id = Guid.NewGuid();
        WorkoutSessionId = workoutSessionId;
        ExerciseId = exerciseId;
        ExerciseName = exerciseName.Trim();
        MetricType = metricType;
        Order = order;
        PerformedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid WorkoutSessionId { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string ExerciseName { get; private set; } = string.Empty;
    public ExerciseMetricType MetricType { get; private set; }
    public int Order { get; private set; }

    // Collection of sets performed for this exercise
    public ICollection<WorkoutSessionSet> Sets { get; private set; } = new List<WorkoutSessionSet>();

    // Additional tracking data
    public double? PerformanceScore { get; private set; } // Optional performance score (0-100)
    public DateTime PerformedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties
    public WorkoutSession WorkoutSession { get; private set; } = null!;

    #region Set Management Methods

    /// <summary>
    /// Add a new set to this exercise
    /// </summary>
    public WorkoutSessionSet AddSet(
        int? repetitions = null,
        double? weight = null,
        int? durationSeconds = null,
        double? distance = null,
        int? restTimeSeconds = null)
    {
        var setNumber = Sets.Count + 1;
        var newSet = new WorkoutSessionSet(
            Id, 
            setNumber, 
            repetitions, 
            weight, 
            durationSeconds, 
            distance, 
            restTimeSeconds);

        Sets.Add(newSet);
        UpdatedAt = DateTime.UtcNow;
        
        return newSet;
    }

    /// <summary>
    /// Update performance of a specific set
    /// </summary>
    public void UpdateSet(
        int setNumber,
        int? repetitions = null,
        double? weight = null,
        int? durationSeconds = null,
        double? distance = null,
        int? restTimeSeconds = null)
    {
        var set = Sets.FirstOrDefault(s => s.SetNumber == setNumber);
        if (set == null)
            throw new TrackingDomainException($"Set number {setNumber} not found");

        set.UpdatePerformance(repetitions, weight, durationSeconds, distance, restTimeSeconds);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove a set from this exercise
    /// </summary>
    public void RemoveSet(int setNumber)
    {
        var set = Sets.FirstOrDefault(s => s.SetNumber == setNumber);
        if (set == null)
            throw new TrackingDomainException($"Set number {setNumber} not found");

        Sets.Remove(set);
        
        // Renumber remaining sets
        var remainingSets = Sets.OrderBy(s => s.SetNumber).ToList();
        for (int i = 0; i < remainingSets.Count; i++)
        {
            // This would require making SetNumber mutable, which we might want to consider
            // For now, let's assume we recreate the sets with proper numbering
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Other Methods

    /// <summary>
    /// Set performance score (0-100)
    /// </summary>
    public void SetPerformanceScore(double score)
    {
        if (score < 0 || score > 100)
            throw new TrackingDomainException("Performance score must be between 0 and 100");

        PerformanceScore = score;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update exercise order
    /// </summary>
    public void UpdateOrder(int newOrder)
    {
        if (newOrder < 1)
            throw new TrackingDomainException("Order must be at least 1");

        Order = newOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Business Methods

    /// <summary>
    /// Get display string for the performance
    /// </summary>
    public string GetPerformanceDisplay()
    {
        if (!Sets.Any())
            return "No sets recorded";

        var parts = new List<string>();

        // Show total sets count
        parts.Add($"{Sets.Count} sets");

        // Aggregate performance across all sets
        var totalReps = Sets.Where(s => s.Repetitions.HasValue).Sum(s => s.Repetitions!.Value);
        var avgWeight = Sets.Where(s => s.Weight.HasValue).Average(s => s.Weight!.Value);
        var totalDuration = Sets.Where(s => s.DurationSeconds.HasValue).Sum(s => s.DurationSeconds!.Value);
        var totalDistance = Sets.Where(s => s.Distance.HasValue).Sum(s => s.Distance!.Value);

        if (totalReps > 0)
            parts.Add($"{totalReps} total reps");

        if (!double.IsNaN(avgWeight) && avgWeight > 0)
            parts.Add($"{avgWeight:F1}kg avg");

        if (totalDuration > 0)
        {
            var minutes = totalDuration / 60;
            var seconds = totalDuration % 60;
            parts.Add(seconds > 0 ? $"{minutes}:{seconds:D2} total" : $"{minutes}min total");
        }

        if (totalDistance > 0)
        {
            if (totalDistance >= 1000)
                parts.Add($"{totalDistance / 1000:F1}km total");
            else
                parts.Add($"{totalDistance:F0}m total");
        }

        return parts.Any() ? string.Join(" | ", parts) : "No data";
    }

    /// <summary>
    /// Calculate estimated calories burned for this exercise
    /// </summary>
    public int EstimateCaloriesBurned(double? userWeightKg = null)
    {
        if (!Sets.Any())
            return 0;

        // Simple estimation based on exercise type and duration/intensity
        var baseCaloriesPerMinute = MetricType switch
        {
            ExerciseMetricType.Time => 8.0,      // Time-based exercises like plank
            ExerciseMetricType.Repetitions => 6.0, // Rep-based exercises
            ExerciseMetricType.Distance => 10.0,   // Running/cycling
            ExerciseMetricType.Weight => 7.0,      // Weight lifting
            _ => 5.0
        };

        // Calculate total duration from sets
        var totalDurationSeconds = Sets.Where(s => s.DurationSeconds.HasValue)
                                      .Sum(s => s.DurationSeconds!.Value);

        var durationMinutes = totalDurationSeconds > 0 
            ? totalDurationSeconds / 60.0 
            : Sets.Count * 2.0; // Rough estimate: 2 minutes per set

        var calories = baseCaloriesPerMinute * durationMinutes;

        // Adjust for user weight if available
        if (userWeightKg.HasValue)
        {
            calories *= (userWeightKg.Value / 70.0); // Normalize to 70kg baseline
        }

        return Math.Max(1, (int)Math.Round(calories));
    }

    /// <summary>
    /// Get the best performance from all sets based on exercise type
    /// Uses intelligent logic to determine what "best" means for this exercise
    /// </summary>
    public double? GetBestPerformance()
    {
        if (!Sets.Any()) return null;

        return MetricType switch
        {
            ExerciseMetricType.Weight => GetBestWeightVolume(),
            ExerciseMetricType.Repetitions => Sets.Where(s => s.Repetitions.HasValue).Max(s => s.Repetitions),
            ExerciseMetricType.Time => Sets.Where(s => s.DurationSeconds.HasValue).Max(s => s.DurationSeconds),
            ExerciseMetricType.Distance => Sets.Where(s => s.Distance.HasValue).Max(s => s.Distance),
            _ => Sets.Select(s => s.GetPrimaryPerformanceValue()).Where(v => v.HasValue).Max()
        };
    }

    /// <summary>
    /// Get best weight × reps volume from all sets
    /// </summary>
    private double? GetBestWeightVolume()
    {
        var volumeSets = Sets.Where(s => s.Weight.HasValue && s.Repetitions.HasValue);
        return volumeSets.Any() 
            ? volumeSets.Max(s => s.Weight!.Value * s.Repetitions!.Value)
            : null;
    }

    /// <summary>
    /// Get overall performance score for this exercise (0-100)
    /// </summary>
    public double GetOverallPerformanceScore()
    {
        if (!Sets.Any()) return 0;

        var scores = Sets.Select(s => s.GetPerformanceScore()).ToList();
        
        // Score moyen avec bonus pour consistance
        var averageScore = scores.Average();
        var consistency = 1.0 - (scores.Max() - scores.Min()) / 100.0; // Pénalité pour variation
        
        return Math.Max(0, averageScore + (consistency * 5)); // Bonus de 5 points max pour consistance
    }

    /// <summary>
    /// Get summary of exercise performance for display
    /// </summary>
    public string GetPerformanceSummary()
    {
        if (!Sets.Any()) return "No sets completed";

        var bestPerformance = GetBestPerformance();
        var totalSets = Sets.Count;
        var avgScore = GetOverallPerformanceScore();

        var summary = $"{totalSets} set{(totalSets > 1 ? "s" : "")}";
        
        if (bestPerformance.HasValue)
        {
            var unit = MetricType switch
            {
                ExerciseMetricType.Weight => GetBestWeightSummary(),
                ExerciseMetricType.Time => $"{bestPerformance:F0}s best",
                ExerciseMetricType.Distance => $"{(bestPerformance >= 1000 ? $"{bestPerformance/1000:F1}km" : $"{bestPerformance:F0}m")} best",
                ExerciseMetricType.Repetitions => $"{bestPerformance:F0} reps best",
                _ => $"{bestPerformance:F1} best"
            };
            
            summary += $" | {unit}";
        }

        summary += $" | {avgScore:F0}% avg";
        return summary;
    }

    /// <summary>
    /// Get best weight performance summary
    /// </summary>
    private string GetBestWeightSummary()
    {
        var bestVolumeSet = Sets
            .Where(s => s.Weight.HasValue && s.Repetitions.HasValue)
            .OrderByDescending(s => s.Weight!.Value * s.Repetitions!.Value)
            .FirstOrDefault();

        return bestVolumeSet != null 
            ? $"{bestVolumeSet.Repetitions}×{bestVolumeSet.Weight:F1}kg best"
            : "No weight data";
    }

    /// <summary>
    /// Get total volume (sets × reps × weight) for strength exercises
    /// </summary>
    public double? GetTotalVolume()
    {
        if (MetricType != ExerciseMetricType.Weight && MetricType != ExerciseMetricType.Repetitions)
            return null;

        var totalVolume = 0.0;
        var hasData = false;

        foreach (var set in Sets.Where(s => s.Repetitions.HasValue && s.Weight.HasValue))
        {
            totalVolume += set.Repetitions!.Value * set.Weight!.Value;
            hasData = true;
        }

        return hasData ? totalVolume : null;
    }

    #endregion
}
