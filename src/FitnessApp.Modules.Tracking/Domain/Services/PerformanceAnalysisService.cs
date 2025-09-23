using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Domain.Services;

/// <summary>
/// Service for calculating intelligent performance scores based on historical data
/// </summary>
public class PerformanceAnalysisService
{
    /// <summary>
    /// Calculate a performance score for an exercise session compared to historical data
    /// </summary>
    /// <param name="currentExercise">The current exercise performance</param>
    /// <param name="historicalExercises">Previous performances of the same exercise</param>
    /// <returns>Performance score from 0-100</returns>
    public double CalculatePerformanceScore(
        WorkoutSessionExercise currentExercise, 
        IEnumerable<WorkoutSessionExercise> historicalExercises)
    {
        if (!currentExercise.Sets.Any())
            return 0;

        var currentBest = currentExercise.GetBestPerformance();
        if (!currentBest.HasValue)
            return 50; // Base score if no measurable performance

        var historicalBestValues = historicalExercises
            .Where(e => e.ExerciseId == currentExercise.ExerciseId)
            .Select(e => e.GetBestPerformance())
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        if (!historicalBestValues.Any())
            return 75; // Good score for first attempt

        return CalculateProgressScore(currentBest.Value, historicalBestValues);
    }

    /// <summary>
    /// Calculate progress score based on current vs historical performance
    /// </summary>
    private static double CalculateProgressScore(double currentValue, List<double> historicalValues)
    {
        var personalBest = historicalValues.Max();
        var recentAverage = historicalValues.TakeLast(5).Average(); // Last 5 sessions
        
        // Score components
        var improvementScore = CalculateImprovementScore(currentValue, personalBest);
        var consistencyScore = CalculateConsistencyScore(currentValue, recentAverage);
        var volumeScore = CalculateVolumeScore(currentValue, historicalValues);
        
        // Weighted average of different aspects
        return (improvementScore * 0.4) + (consistencyScore * 0.3) + (volumeScore * 0.3);
    }

    /// <summary>
    /// Score based on improvement vs personal best
    /// </summary>
    private static double CalculateImprovementScore(double current, double personalBest)
    {
        if (current >= personalBest)
            return 100; // New personal record!

        var percentageOfBest = current / personalBest;
        
        return percentageOfBest switch
        {
            >= 0.95 => 90, // Within 5% of PR
            >= 0.90 => 80, // Within 10% of PR  
            >= 0.85 => 70, // Within 15% of PR
            >= 0.80 => 60, // Within 20% of PR
            >= 0.70 => 50, // Within 30% of PR
            _ => Math.Max(20, percentageOfBest * 50) // Minimum 20 points
        };
    }

    /// <summary>
    /// Score based on consistency with recent performances
    /// </summary>
    private static double CalculateConsistencyScore(double current, double recentAverage)
    {
        var ratio = current / recentAverage;
        
        return ratio switch
        {
            >= 1.10 => 100, // 10%+ better than recent average
            >= 1.05 => 85,  // 5%+ better than recent average
            >= 0.95 => 75,  // Within 5% of recent average
            >= 0.90 => 65,  // Within 10% of recent average
            >= 0.85 => 55,  // Within 15% of recent average
            _ => Math.Max(30, ratio * 50) // Minimum 30 points
        };
    }

    /// <summary>
    /// Score based on total volume/effort for the session
    /// </summary>
    private static double CalculateVolumeScore(double current, List<double> historicalValues)
    {
        var averageHistorical = historicalValues.Average();
        var ratio = current / averageHistorical;
        
        return ratio switch
        {
            >= 1.20 => 100, // 20%+ more volume than average
            >= 1.10 => 90,  // 10%+ more volume than average
            >= 1.00 => 80,  // Equal to average volume
            >= 0.90 => 70,  // Within 10% of average volume
            >= 0.80 => 60,  // Within 20% of average volume
            _ => Math.Max(40, ratio * 60) // Minimum 40 points
        };
    }

    /// <summary>
    /// Get a text description of the performance level
    /// </summary>
    public static string GetPerformanceDescription(double score)
    {
        return score switch
        {
            >= 95 => "🔥 Personal Record!",
            >= 85 => "💪 Excellent",
            >= 75 => "✅ Great",
            >= 65 => "👍 Good",
            >= 55 => "👌 Solid",
            >= 45 => "📈 Improving",
            >= 35 => "🎯 On Track",
            _ => "💪 Keep Going"
        };
    }

    /// <summary>
    /// Calculate overall workout session score
    /// </summary>
    public double CalculateWorkoutScore(WorkoutSession session, 
        Dictionary<Guid, IEnumerable<WorkoutSessionExercise>> historicalDataByExercise)
    {
        if (!session.Exercises.Any())
            return 0;

        var exerciseScores = new List<double>();

        foreach (var exercise in session.Exercises)
        {
            var historicalData = historicalDataByExercise.GetValueOrDefault(exercise.ExerciseId, Enumerable.Empty<WorkoutSessionExercise>());
            var exerciseScore = CalculatePerformanceScore(exercise, historicalData);
            exerciseScores.Add(exerciseScore);
        }

        // Weighted average with bonus for completing all exercises
        var averageScore = exerciseScores.Average();
        var completionBonus = session.Exercises.All(e => e.Sets.Any()) ? 5.0 : 0.0;

        return Math.Min(100, averageScore + completionBonus);
    }
}
