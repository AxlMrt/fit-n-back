using AutoMapper;
using FitnessApp.Modules.Tracking.Application.Interfaces;
using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.Modules.Tracking.Domain.Exceptions;
using FitnessApp.Modules.Tracking.Domain.Repositories;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Tracking.Application.Services;

/// <summary>
/// Service implementation for tracking workout sessions and user metrics
/// </summary>
public class TrackingService : ITrackingService
{
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly IUserMetricRepository _userMetricRepository;
    private readonly IPlannedWorkoutRepository _plannedWorkoutRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<TrackingService> _logger;

    public TrackingService(
        IWorkoutSessionRepository workoutSessionRepository,
        IUserMetricRepository userMetricRepository,
        IPlannedWorkoutRepository plannedWorkoutRepository,
        IMapper mapper,
        ILogger<TrackingService> logger)
    {
        _workoutSessionRepository = workoutSessionRepository ?? throw new ArgumentNullException(nameof(workoutSessionRepository));
        _userMetricRepository = userMetricRepository ?? throw new ArgumentNullException(nameof(userMetricRepository));
        _plannedWorkoutRepository = plannedWorkoutRepository ?? throw new ArgumentNullException(nameof(plannedWorkoutRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Workout Sessions

    public async Task<WorkoutSessionDto> StartWorkoutSessionAsync(Guid userId, Guid workoutId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting workout session for user {UserId} with workout {WorkoutId}", userId, workoutId);

        // Check if user already has an active session
        var existingSession = await _workoutSessionRepository.GetActiveSessionAsync(userId, cancellationToken);
        if (existingSession != null)
        {
            throw TrackingDomainException.UserAlreadyHasActiveWorkoutSession();
        }

        var session = new WorkoutSession(userId, workoutId);
        await _workoutSessionRepository.AddAsync(session, cancellationToken);

        _logger.LogInformation("Workout session {SessionId} started successfully", session.Id);
        return _mapper.Map<WorkoutSessionDto>(session);
    }

    public async Task<WorkoutSessionDto> CompleteWorkoutSessionAsync(Guid sessionId, PerceivedDifficulty perceivedDifficulty, string? notes = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Completing workout session {SessionId}", sessionId);

        var session = await _workoutSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Workout session with ID {sessionId} not found");
        }

        session.Complete(perceivedDifficulty, notes);
        await _workoutSessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Workout session {SessionId} completed successfully", sessionId);
        return _mapper.Map<WorkoutSessionDto>(session);
    }

    public async Task<WorkoutSessionDto> AbandonWorkoutSessionAsync(Guid sessionId, string? reason = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Abandoning workout session {SessionId}", sessionId);

        var session = await _workoutSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Workout session with ID {sessionId} not found");
        }

        session.Abandon(reason);
        await _workoutSessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Workout session {SessionId} abandoned", sessionId);
        return _mapper.Map<WorkoutSessionDto>(session);
    }

    public async Task<WorkoutSessionDto> AddExerciseToSessionAsync(Guid sessionId, Guid exerciseId, string exerciseName, ExerciseMetricType metricType, int? repetitions = null, double? weight = null, int? durationSeconds = null, double? distance = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding exercise {ExerciseId} to session {SessionId}", exerciseId, sessionId);

        var session = await _workoutSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Workout session with ID {sessionId} not found");
        }

        // Add exercise with initial set if performance data provided
        session.AddExercise(exerciseId, exerciseName, metricType, null, repetitions, weight, durationSeconds, distance);
        await _workoutSessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Exercise {ExerciseId} added to session {SessionId} successfully", exerciseId, sessionId);
        return _mapper.Map<WorkoutSessionDto>(session);
    }

    public async Task<WorkoutSessionSetDto> AddSetToExerciseAsync(Guid sessionId, Guid exerciseId, int? repetitions = null, double? weight = null, int? durationSeconds = null, double? distance = null, int? restTimeSeconds = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding set to exercise {ExerciseId} in session {SessionId}", exerciseId, sessionId);

        var session = await _workoutSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Workout session with ID {sessionId} not found");
        }

        var exercise = session.GetExercise(exerciseId);
        if (exercise == null)
        {
            throw new InvalidOperationException($"Exercise with ID {exerciseId} not found in session");
        }

        var newSet = exercise.AddSet(repetitions, weight, durationSeconds, distance, restTimeSeconds);
        await _workoutSessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Set added successfully to exercise {ExerciseId}", exerciseId);
        return _mapper.Map<WorkoutSessionSetDto>(newSet);
    }

    public async Task<WorkoutSessionDto> RemoveExerciseFromSessionAsync(Guid sessionId, Guid exerciseId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Removing exercise {ExerciseId} from session {SessionId}", exerciseId, sessionId);

        var session = await _workoutSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session == null)
        {
            throw new InvalidOperationException($"Workout session with ID {sessionId} not found");
        }

        session.RemoveExercise(exerciseId);
        await _workoutSessionRepository.UpdateAsync(session, cancellationToken);

        _logger.LogInformation("Exercise {ExerciseId} removed from session {SessionId} successfully", exerciseId, sessionId);
        return _mapper.Map<WorkoutSessionDto>(session);
    }

    public async Task<WorkoutSessionDto?> GetWorkoutSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _workoutSessionRepository.GetByIdAsync(sessionId, cancellationToken);
        return session != null ? _mapper.Map<WorkoutSessionDto>(session) : null;
    }

    public async Task<IEnumerable<WorkoutSessionListDto>> GetUserWorkoutHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var sessions = await _workoutSessionRepository.GetUserWorkoutHistoryAsync(userId, cancellationToken: cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutSessionListDto>>(sessions);
    }

    public async Task<IEnumerable<WorkoutSessionListDto>> GetWorkoutSessionsInPeriodAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var sessions = await _workoutSessionRepository.GetSessionsInPeriodAsync(userId, startDate, endDate, cancellationToken);
        return _mapper.Map<IEnumerable<WorkoutSessionListDto>>(sessions);
    }

    public async Task<WorkoutSessionDto?> GetActiveWorkoutSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var session = await _workoutSessionRepository.GetActiveSessionAsync(userId, cancellationToken);
        return session != null ? _mapper.Map<WorkoutSessionDto>(session) : null;
    }

    #endregion

    #region User Metrics

    public async Task<UserMetricDto> RecordUserMetricAsync(Guid userId, UserMetricType metricType, double value, DateTime? recordedAt = null, string? notes = null, string? unit = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recording metric {MetricType} for user {UserId}", metricType, userId);

        var metric = new UserMetric(userId, metricType, value, recordedAt, notes, unit);
        await _userMetricRepository.AddAsync(metric, cancellationToken);

        _logger.LogInformation("Metric {MetricType} recorded successfully for user {UserId}", metricType, userId);
        return _mapper.Map<UserMetricDto>(metric);
    }

    public async Task<UserMetricDto> UpdateUserMetricAsync(Guid metricId, double value, string? notes = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating metric {MetricId}", metricId);

        var metric = await _userMetricRepository.GetByIdAsync(metricId, cancellationToken);
        if (metric == null)
        {
            throw TrackingDomainException.MetricNotFound(metricId);
        }

        metric.UpdateValue(value, notes);
        await _userMetricRepository.UpdateAsync(metric, cancellationToken);

        _logger.LogInformation("Metric {MetricId} updated successfully", metricId);
        return _mapper.Map<UserMetricDto>(metric);
    }

    public async Task<IEnumerable<UserMetricDto>> GetUserMetricsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var metrics = await _userMetricRepository.GetByUserIdAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<UserMetricDto>>(metrics);
    }

    public async Task<IEnumerable<UserMetricDto>> GetUserMetricsByTypeAsync(Guid userId, UserMetricType metricType, CancellationToken cancellationToken = default)
    {
        var metrics = await _userMetricRepository.GetByUserAndTypeAsync(userId, metricType, cancellationToken);
        return _mapper.Map<IEnumerable<UserMetricDto>>(metrics);
    }

    public async Task<UserMetricDto?> GetLatestMetricAsync(Guid userId, UserMetricType metricType, CancellationToken cancellationToken = default)
    {
        var metric = await _userMetricRepository.GetLatestMetricAsync(userId, metricType, cancellationToken);
        return metric != null ? _mapper.Map<UserMetricDto>(metric) : null;
    }

    public async Task DeleteUserMetricAsync(Guid metricId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting metric {MetricId}", metricId);

        var metric = await _userMetricRepository.GetByIdAsync(metricId, cancellationToken);
        if (metric == null)
        {
            throw TrackingDomainException.MetricNotFound(metricId);
        }

        await _userMetricRepository.DeleteAsync(metric, cancellationToken);
        _logger.LogInformation("Metric {MetricId} deleted successfully", metricId);
    }

    #endregion

    #region Planned Workouts

    public async Task<PlannedWorkoutDto> ScheduleWorkoutAsync(Guid userId, Guid workoutId, DateTime scheduledDate, bool isFromProgram = false, Guid? programId = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Scheduling workout {WorkoutId} for user {UserId} on {ScheduledDate}", workoutId, userId, scheduledDate);

        // Check if workout is already scheduled for this date
        var exists = await _plannedWorkoutRepository.HasPlannedWorkoutAsync(userId, workoutId, scheduledDate, cancellationToken);
        if (exists)
        {
            throw TrackingDomainException.WorkoutAlreadyScheduled(scheduledDate);
        }

        var plannedWorkout = new PlannedWorkout(userId, workoutId, scheduledDate, isFromProgram, programId);
        await _plannedWorkoutRepository.AddAsync(plannedWorkout, cancellationToken);

        _logger.LogInformation("Workout {WorkoutId} scheduled successfully for {ScheduledDate}", workoutId, scheduledDate);
        return _mapper.Map<PlannedWorkoutDto>(plannedWorkout);
    }

    public async Task<PlannedWorkoutDto> RescheduleWorkoutAsync(Guid plannedWorkoutId, DateTime newScheduledDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rescheduling planned workout {PlannedWorkoutId} to {NewDate}", plannedWorkoutId, newScheduledDate);

        var plannedWorkout = await _plannedWorkoutRepository.GetByIdAsync(plannedWorkoutId, cancellationToken);
        if (plannedWorkout == null)
        {
            throw new InvalidOperationException($"Planned workout with ID {plannedWorkoutId} not found");
        }

        plannedWorkout.Reschedule(newScheduledDate);
        await _plannedWorkoutRepository.UpdateAsync(plannedWorkout, cancellationToken);

        _logger.LogInformation("Planned workout {PlannedWorkoutId} rescheduled successfully", plannedWorkoutId);
        return _mapper.Map<PlannedWorkoutDto>(plannedWorkout);
    }

    public async Task CancelPlannedWorkoutAsync(Guid plannedWorkoutId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cancelling planned workout {PlannedWorkoutId}", plannedWorkoutId);

        var plannedWorkout = await _plannedWorkoutRepository.GetByIdAsync(plannedWorkoutId, cancellationToken);
        if (plannedWorkout == null)
        {
            throw new InvalidOperationException($"Planned workout with ID {plannedWorkoutId} not found");
        }

        plannedWorkout.Cancel();
        await _plannedWorkoutRepository.UpdateAsync(plannedWorkout, cancellationToken);

        _logger.LogInformation("Planned workout {PlannedWorkoutId} cancelled successfully", plannedWorkoutId);
    }

    public async Task<IEnumerable<PlannedWorkoutDto>> GetUpcomingWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _plannedWorkoutRepository.GetUpcomingAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<PlannedWorkoutDto>>(workouts);
    }

    public async Task<IEnumerable<PlannedWorkoutDto>> GetOverdueWorkoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var workouts = await _plannedWorkoutRepository.GetOverdueAsync(userId, cancellationToken);
        return _mapper.Map<IEnumerable<PlannedWorkoutDto>>(workouts);
    }

    public async Task<IEnumerable<PlannedWorkoutDto>> GetPlannedWorkoutsForDateAsync(Guid userId, DateTime date, CancellationToken cancellationToken = default)
    {
        var workouts = await _plannedWorkoutRepository.GetByDateAsync(userId, date, cancellationToken);
        return _mapper.Map<IEnumerable<PlannedWorkoutDto>>(workouts);
    }

    #endregion

    #region Statistics and Analytics

    public async Task<TrackingStatsDto> GetUserTrackingStatsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating tracking statistics for user {UserId}", userId);

        var completedSessions = await _workoutSessionRepository.GetSessionsByStatusAsync(userId, WorkoutSessionStatus.Completed, cancellationToken);
        var allSessions = completedSessions.ToList();

        // Basic stats
        var totalWorkouts = allSessions.Count;
        var totalWorkoutTime = allSessions.Sum(s => s.TotalDurationSeconds ?? 0);
        var averageWorkoutDuration = totalWorkouts > 0 ? totalWorkoutTime / totalWorkouts : 0;
        var totalCaloriesBurned = allSessions.Sum(s => s.CaloriesEstimated ?? 0);

        // Weekly and monthly stats
        var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
        var oneMonthAgo = DateTime.UtcNow.AddDays(-30);
        
        var workoutsThisWeek = allSessions.Count(s => s.EndTime >= oneWeekAgo);
        var workoutsThisMonth = allSessions.Count(s => s.EndTime >= oneMonthAgo);

        // Average perceived difficulty
        var sessionsWithDifficulty = allSessions.Where(s => s.PerceivedDifficulty.HasValue).ToList();
        var averagePerceivedDifficulty = sessionsWithDifficulty.Any() 
            ? sessionsWithDifficulty.Average(s => (int)s.PerceivedDifficulty!.Value) 
            : (double?)null;

        // Last workout date
        var lastWorkoutDate = allSessions.OrderByDescending(s => s.EndTime).FirstOrDefault()?.EndTime;

        // Streak calculation (simplified)
        var currentStreak = CalculateCurrentStreak(allSessions);
        var longestStreak = CalculateLongestStreak(allSessions);

        // Weekly frequency (last 8 weeks)
        var weeklyFrequency = CalculateWeeklyFrequency(allSessions);

        // Metric trends (basic implementation)
        var metricTrends = await CalculateMetricTrends(userId, cancellationToken);

        return new TrackingStatsDto(
            totalWorkouts,
            totalWorkoutTime,
            averageWorkoutDuration,
            totalCaloriesBurned,
            workoutsThisWeek,
            workoutsThisMonth,
            averagePerceivedDifficulty,
            lastWorkoutDate,
            currentStreak,
            longestStreak,
            weeklyFrequency,
            metricTrends);
    }

    public async Task<ExercisePerformanceDto?> GetExercisePerformanceAsync(Guid userId, Guid exerciseId, CancellationToken cancellationToken = default)
    {
        var userSessions = await _workoutSessionRepository.GetByUserIdAsync(userId, cancellationToken);
        var exercisePerformances = userSessions
            .SelectMany(s => s.Exercises)
            .Where(e => e.ExerciseId == exerciseId)
            .OrderByDescending(e => e.PerformedAt)
            .ToList();

        if (!exercisePerformances.Any())
            return null;

        var firstExercise = exercisePerformances.First();
        
        // Get best performance values from sets across all sessions
        var bestPerformanceValues = exercisePerformances
            .SelectMany(e => e.Sets)
            .Select(s => GetPerformanceValue(s, firstExercise.MetricType))
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .ToList();

        var performance = new ExercisePerformanceDto(
            exerciseId,
            firstExercise.ExerciseName,
            firstExercise.MetricType,
            bestPerformanceValues.Any() ? bestPerformanceValues.Max() : null,
            bestPerformanceValues.Any() ? bestPerformanceValues.Average() : null,
            exercisePerformances.Count,
            exercisePerformances.First().PerformedAt,
            exercisePerformances.Take(10).Select(e => new PerformanceHistoryDto(
                e.PerformedAt,
                GetBestSetPerformance(e, firstExercise.MetricType) ?? 0,
                null)).ToList()); // Supprimé les notes des exercices

        return performance;
    }

    public async Task<IEnumerable<WorkoutFrequencyDto>> GetWorkoutFrequencyAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var sessions = await _workoutSessionRepository.GetSessionsInPeriodAsync(userId, startDate, endDate, cancellationToken);
        
        return sessions
            .Where(s => s.Status == WorkoutSessionStatus.Completed && s.EndTime.HasValue)
            .GroupBy(s => s.EndTime!.Value.Date)
            .Select(g => new WorkoutFrequencyDto(
                g.Key,
                g.Count(),
                g.Sum(s => s.TotalDurationSeconds ?? 0)))
            .OrderBy(f => f.Date);
    }

    #endregion

    #region Private Helper Methods

    private static int CalculateCurrentStreak(List<WorkoutSession> sessions)
    {
        if (!sessions.Any()) return 0;

        var sortedSessions = sessions.OrderByDescending(s => s.EndTime ?? s.CreatedAt).ToList();
        var currentDate = DateTime.UtcNow.Date;
        var streak = 0;

        foreach (var session in sortedSessions)
        {
            var sessionDate = (session.EndTime ?? session.CreatedAt).Date;
            
            if (sessionDate == currentDate || sessionDate == currentDate.AddDays(-1))
            {
                streak++;
                currentDate = sessionDate.AddDays(-1);
            }
            else
            {
                break;
            }
        }

        return streak;
    }

    private static int CalculateLongestStreak(List<WorkoutSession> sessions)
    {
        if (!sessions.Any()) return 0;

        var sessionDates = sessions
            .Select(s => (s.EndTime ?? s.CreatedAt).Date)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var longestStreak = 1;
        var currentStreak = 1;

        for (int i = 1; i < sessionDates.Count; i++)
        {
            if (sessionDates[i] == sessionDates[i - 1].AddDays(1))
            {
                currentStreak++;
            }
            else
            {
                longestStreak = Math.Max(longestStreak, currentStreak);
                currentStreak = 1;
            }
        }

        return Math.Max(longestStreak, currentStreak);
    }

    private static List<WorkoutFrequencyDto> CalculateWeeklyFrequency(List<WorkoutSession> sessions)
    {
        var eightWeeksAgo = DateTime.UtcNow.AddDays(-56).Date;
        var recentSessions = sessions.Where(s => s.EndTime?.Date >= eightWeeksAgo).ToList();

        return recentSessions
            .GroupBy(s => GetWeekStart(s.EndTime ?? s.CreatedAt))
            .Select(g => new WorkoutFrequencyDto(
                g.Key,
                g.Count(),
                g.Sum(s => s.TotalDurationSeconds ?? 0)))
            .OrderBy(f => f.Date)
            .ToList();
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private async Task<List<MetricTrendDto>> CalculateMetricTrends(Guid userId, CancellationToken cancellationToken)
    {
        var trends = new List<MetricTrendDto>();
        
        // Get trends for simplified metric types (focus on what matters)
        var metricTypes = new[] { UserMetricType.Weight, UserMetricType.PersonalRecord };
        
        foreach (var metricType in metricTypes)
        {
            var metrics = await _userMetricRepository.GetLatestByTypeAsync(userId, metricType, 2, cancellationToken);
            var metricsList = metrics.ToList();

            if (metricsList.Any())
            {
                var current = metricsList.First();
                var previous = metricsList.Count > 1 ? metricsList[1] : null;

                var percentageChange = previous != null && previous.Value != 0 
                    ? ((current.Value - previous.Value) / previous.Value) * 100 
                    : (double?)null;

                var trend = percentageChange switch
                {
                    > 2 => "up",
                    < -2 => "down",
                    _ => "stable"
                };

                trends.Add(new MetricTrendDto(
                    metricType,
                    current.Value,
                    previous?.Value,
                    percentageChange,
                    trend,
                    current.RecordedAt));
            }
        }

        return trends;
    }

    #region Helper Methods

    /// <summary>
    /// Get performance value from a set based on metric type
    /// </summary>
    private static double? GetPerformanceValue(WorkoutSessionSet set, ExerciseMetricType metricType)
    {
        return metricType switch
        {
            ExerciseMetricType.Weight => set.Weight,
            ExerciseMetricType.Repetitions => set.Repetitions,
            ExerciseMetricType.Time => set.DurationSeconds,
            ExerciseMetricType.Distance => set.Distance,
            _ => null
        };
    }

    /// <summary>
    /// Get the best set performance for an exercise based on metric type
    /// </summary>
    private static double? GetBestSetPerformance(WorkoutSessionExercise exercise, ExerciseMetricType metricType)
    {
        if (!exercise.Sets.Any())
            return null;

        return metricType switch
        {
            ExerciseMetricType.Weight => exercise.Sets.Where(s => s.Weight.HasValue).Max(s => s.Weight),
            ExerciseMetricType.Repetitions => exercise.Sets.Where(s => s.Repetitions.HasValue).Max(s => s.Repetitions),
            ExerciseMetricType.Time => exercise.Sets.Where(s => s.DurationSeconds.HasValue).Max(s => s.DurationSeconds),
            ExerciseMetricType.Distance => exercise.Sets.Where(s => s.Distance.HasValue).Max(s => s.Distance),
            _ => null
        };
    }

    #endregion

    #endregion
}
