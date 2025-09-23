using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Application.Interfaces;

/// <summary>
/// Service interface for tracking workout sessions and user metrics
/// </summary>
public interface ITrackingService
{
    #region Workout Sessions

    /// <summary>
    /// Start a new workout session
    /// </summary>
    Task<WorkoutSessionDto> StartWorkoutSessionAsync(
        Guid userId, 
        Guid workoutId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete an active workout session
    /// </summary>
    Task<WorkoutSessionDto> CompleteWorkoutSessionAsync(
        Guid sessionId, 
        PerceivedDifficulty perceivedDifficulty, 
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Abandon an active workout session
    /// </summary>
    Task<WorkoutSessionDto> AbandonWorkoutSessionAsync(
        Guid sessionId, 
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add exercise performance to a session
    /// </summary>
    Task<WorkoutSessionDto> AddExerciseToSessionAsync(
        Guid sessionId,
        Guid exerciseId,
        string exerciseName,
        ExerciseMetricType metricType,
        int? repetitions = null,
        double? weight = null,
        int? durationSeconds = null,
        double? distance = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new set to an exercise in a session
    /// </summary>
    Task<WorkoutSessionSetDto> AddSetToExerciseAsync(
        Guid sessionId,
        Guid exerciseId,
        int? repetitions = null,
        double? weight = null,
        int? durationSeconds = null,
        double? distance = null,
        int? restTimeSeconds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove exercise from a session
    /// </summary>
    Task<WorkoutSessionDto> RemoveExerciseFromSessionAsync(
        Guid sessionId,
        Guid exerciseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workout session by ID
    /// </summary>
    Task<WorkoutSessionDto?> GetWorkoutSessionAsync(
        Guid sessionId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's workout history
    /// </summary>
    Task<IEnumerable<WorkoutSessionListDto>> GetUserWorkoutHistoryAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workout sessions in a date range
    /// </summary>
    Task<IEnumerable<WorkoutSessionListDto>> GetWorkoutSessionsInPeriodAsync(
        Guid userId, 
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active workout session for user
    /// </summary>
    Task<WorkoutSessionDto?> GetActiveWorkoutSessionAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    #endregion

    #region User Metrics

    /// <summary>
    /// Record a new user metric
    /// </summary>
    Task<UserMetricDto> RecordUserMetricAsync(
        Guid userId,
        UserMetricType metricType,
        double value,
        DateTime? recordedAt = null,
        string? notes = null,
        string? unit = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing user metric
    /// </summary>
    Task<UserMetricDto> UpdateUserMetricAsync(
        Guid metricId,
        double value,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all user metrics
    /// </summary>
    Task<IEnumerable<UserMetricDto>> GetUserMetricsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user metrics by type
    /// </summary>
    Task<IEnumerable<UserMetricDto>> GetUserMetricsByTypeAsync(
        Guid userId, 
        UserMetricType metricType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest metric value by type
    /// </summary>
    Task<UserMetricDto?> GetLatestMetricAsync(
        Guid userId, 
        UserMetricType metricType, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a user metric
    /// </summary>
    Task DeleteUserMetricAsync(
        Guid metricId, 
        CancellationToken cancellationToken = default);

    #endregion

    #region Planned Workouts

    /// <summary>
    /// Schedule a workout for a specific date
    /// </summary>
    Task<PlannedWorkoutDto> ScheduleWorkoutAsync(
        Guid userId,
        Guid workoutId,
        DateTime scheduledDate,
        bool isFromProgram = false,
        Guid? programId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reschedule a planned workout
    /// </summary>
    Task<PlannedWorkoutDto> RescheduleWorkoutAsync(
        Guid plannedWorkoutId,
        DateTime newScheduledDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a planned workout
    /// </summary>
    Task CancelPlannedWorkoutAsync(
        Guid plannedWorkoutId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming workouts for user
    /// </summary>
    Task<IEnumerable<PlannedWorkoutDto>> GetUpcomingWorkoutsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overdue workouts for user
    /// </summary>
    Task<IEnumerable<PlannedWorkoutDto>> GetOverdueWorkoutsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get planned workouts for a specific date
    /// </summary>
    Task<IEnumerable<PlannedWorkoutDto>> GetPlannedWorkoutsForDateAsync(
        Guid userId, 
        DateTime date, 
        CancellationToken cancellationToken = default);

    #endregion

    #region Statistics and Analytics

    /// <summary>
    /// Get comprehensive tracking statistics for user
    /// </summary>
    Task<TrackingStatsDto> GetUserTrackingStatsAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get exercise performance history
    /// </summary>
    Task<ExercisePerformanceDto?> GetExercisePerformanceAsync(
        Guid userId,
        Guid exerciseId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workout frequency data
    /// </summary>
    Task<IEnumerable<WorkoutFrequencyDto>> GetWorkoutFrequencyAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    #endregion
}
