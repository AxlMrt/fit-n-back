using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FitnessApp.Modules.Tracking.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.SharedKernel.Enums;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.API.Infrastructure.Errors;
using FitnessApp.API.Controllers;
using System.Security.Claims;

namespace FitnessApp.API.Controllers.v1;

/// <summary>
/// API controller for tracking workout sessions and user metrics
/// </summary>
[Authorize]
[Route("api/v1/tracking")]
public class TrackingController : BaseController
{
    private readonly ITrackingService _trackingService;
    private readonly IExerciseService _exerciseService;

    public TrackingController(ITrackingService trackingService, IExerciseService exerciseService)
    {
        _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));
        _exerciseService = exerciseService ?? throw new ArgumentNullException(nameof(exerciseService));
    }

    private Guid GetUserId() => GetCurrentUserId();

    #region Workout Sessions

    /// <summary>
    /// Start a new workout session
    /// </summary>
    [HttpPost("sessions/start")]
    [ProducesResponseType(typeof(WorkoutSessionDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> StartWorkoutSession([FromBody] StartWorkoutSessionRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var session = await _trackingService.StartWorkoutSessionAsync(
            userId, 
            request.WorkoutId, 
            cancellationToken);

        return Ok(session);
    }

    /// <summary>
    /// Complete a workout session
    /// </summary>
    [HttpPost("sessions/{sessionId:guid}/complete")]
    [ProducesResponseType(typeof(WorkoutSessionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CompleteWorkoutSession(
        Guid sessionId, 
        [FromBody] CompleteWorkoutSessionRequest request, 
        CancellationToken cancellationToken = default)
    {
        var session = await _trackingService.CompleteWorkoutSessionAsync(
            sessionId, 
            request.PerceivedDifficulty, 
            request.Notes,
            cancellationToken);

        return Ok(session);
    }

    /// <summary>
    /// Abandon a workout session
    /// </summary>
    [HttpPost("sessions/{sessionId:guid}/abandon")]
    [ProducesResponseType(typeof(WorkoutSessionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AbandonWorkoutSession(
        Guid sessionId, 
        [FromBody] string? reason = null, 
        CancellationToken cancellationToken = default)
    {
        var session = await _trackingService.AbandonWorkoutSessionAsync(sessionId, reason, cancellationToken);
        return Ok(session);
    }

    /// <summary>
    /// Add exercise performance to a session
    /// </summary>
    [HttpPost("sessions/{sessionId:guid}/exercises")]
    [ProducesResponseType(typeof(WorkoutSessionDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddExerciseToSession(
        Guid sessionId, 
        [FromBody] AddExercisePerformanceRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Get exercise name from exercise service
        var exercise = await _exerciseService.GetByIdAsync(request.ExerciseId, cancellationToken);
        if (exercise == null)
        {
            return BadRequest(new { Message = $"Exercise with ID {request.ExerciseId} not found" });
        }

        var session = await _trackingService.AddExerciseToSessionAsync(
            sessionId,
            request.ExerciseId,
            exercise.Name,
            request.MetricType,
            request.Repetitions,
            request.Weight,
            request.DurationSeconds,
            request.Distance,
            cancellationToken);

        return Ok(session);
    }

    /// <summary>
    /// Add a new set to an exercise
    /// </summary>
    [HttpPost("sessions/{sessionId:guid}/exercises/{exerciseId:guid}/sets")]
    [ProducesResponseType(typeof(WorkoutSessionSetDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AddSetToExercise(
        Guid sessionId,
        Guid exerciseId,
        [FromBody] AddSetRequest request,
        CancellationToken cancellationToken = default)
    {
        var set = await _trackingService.AddSetToExerciseAsync(
            sessionId, 
            exerciseId, 
            request.Repetitions, 
            request.Weight, 
            request.DurationSeconds, 
            request.Distance, 
            request.RestTimeSeconds, 
            cancellationToken);
        return Ok(set);
    }

    /// <summary>
    /// Remove exercise from a session
    /// </summary>
    [HttpDelete("sessions/{sessionId:guid}/exercises/{exerciseId:guid}")]
    [ProducesResponseType(typeof(WorkoutSessionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveExerciseFromSession(
        Guid sessionId,
        Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        var session = await _trackingService.RemoveExerciseFromSessionAsync(sessionId, exerciseId, cancellationToken);
        return Ok(session);
    }

    /// <summary>
    /// Get user's workout history
    /// </summary>
    [HttpGet("sessions/history")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutSessionListDto>), 200)]
    public async Task<IActionResult> GetWorkoutHistory(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var history = await _trackingService.GetUserWorkoutHistoryAsync(userId, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Get specific workout session details
    /// </summary>
    [HttpGet("sessions/{sessionId:guid}")]
    [ProducesResponseType(typeof(WorkoutSessionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetWorkoutSession(Guid sessionId, CancellationToken cancellationToken = default)
    {
        var session = await _trackingService.GetWorkoutSessionAsync(sessionId, cancellationToken);
        if (session == null)
            return NotFound(new { Message = $"Workout session with ID {sessionId} not found" });

        return Ok(session);
    }

    /// <summary>
    /// Get active workout session for user
    /// </summary>
    [HttpGet("sessions/active")]
    [ProducesResponseType(typeof(WorkoutSessionDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetActiveWorkoutSession(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var session = await _trackingService.GetActiveWorkoutSessionAsync(userId, cancellationToken);
        if (session == null)
            return NotFound(new { Message = "No active workout session found" });

        return Ok(session);
    }

    #endregion

    #region User Metrics

    /// <summary>
    /// Record a user metric (weight, strength records, etc.)
    /// </summary>
    [HttpPost("metrics")]
    [ProducesResponseType(typeof(UserMetricDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> RecordUserMetric([FromBody] RecordUserMetricRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var metric = await _trackingService.RecordUserMetricAsync(
            userId,
            request.MetricType,
            request.Value,
            request.RecordedAt,
            request.Notes,
            request.Unit,
            cancellationToken);

        return Ok(metric);
    }

    /// <summary>
    /// Update existing user metric
    /// </summary>
    [HttpPut("metrics/{metricId:guid}")]
    [ProducesResponseType(typeof(UserMetricDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateUserMetric(
        Guid metricId,
        [FromBody] RecordUserMetricRequest request,
        CancellationToken cancellationToken = default)
    {
        var metric = await _trackingService.UpdateUserMetricAsync(metricId, request.Value, request.Notes, cancellationToken);
        return Ok(metric);
    }

    /// <summary>
    /// Get all user metrics
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(IEnumerable<UserMetricDto>), 200)]
    public async Task<IActionResult> GetUserMetrics(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var metrics = await _trackingService.GetUserMetricsAsync(userId, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get user metrics by type
    /// </summary>
    [HttpGet("metrics/{metricType}")]
    [ProducesResponseType(typeof(IEnumerable<UserMetricDto>), 200)]
    public async Task<IActionResult> GetUserMetricsByType(UserMetricType metricType, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var metrics = await _trackingService.GetUserMetricsByTypeAsync(userId, metricType, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get latest metric value by type
    /// </summary>
    [HttpGet("metrics/{metricType}/latest")]
    [ProducesResponseType(typeof(UserMetricDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLatestMetric(UserMetricType metricType, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var metric = await _trackingService.GetLatestMetricAsync(userId, metricType, cancellationToken);
        if (metric == null)
            return NotFound(new { Message = $"No {metricType} metric found for user" });

        return Ok(metric);
    }

    /// <summary>
    /// Delete a user metric
    /// </summary>
    [HttpDelete("metrics/{metricId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteUserMetric(Guid metricId, CancellationToken cancellationToken = default)
    {
        await _trackingService.DeleteUserMetricAsync(metricId, cancellationToken);
        return NoContent();
    }

    #endregion

    #region Planned Workouts

    /// <summary>
    /// Schedule a workout for a specific date
    /// </summary>
    [HttpPost("planned-workouts")]
    [ProducesResponseType(typeof(PlannedWorkoutDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ScheduleWorkout([FromBody] ScheduleWorkoutRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var plannedWorkout = await _trackingService.ScheduleWorkoutAsync(
            userId,
            request.WorkoutId,
            request.ScheduledDate,
            request.IsFromProgram,
            request.ProgramId,
            cancellationToken);

        return Ok(plannedWorkout);
    }

    /// <summary>
    /// Reschedule a planned workout
    /// </summary>
    [HttpPut("planned-workouts/{plannedWorkoutId:guid}/reschedule")]
    [ProducesResponseType(typeof(PlannedWorkoutDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RescheduleWorkout(
        Guid plannedWorkoutId,
        [FromBody] RescheduleWorkoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var plannedWorkout = await _trackingService.RescheduleWorkoutAsync(
            plannedWorkoutId,
            request.NewScheduledDate,
            cancellationToken);
        return Ok(plannedWorkout);
    }

    /// <summary>
    /// Cancel a planned workout
    /// </summary>
    [HttpDelete("planned-workouts/{plannedWorkoutId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CancelPlannedWorkout(Guid plannedWorkoutId, CancellationToken cancellationToken = default)
    {
        await _trackingService.CancelPlannedWorkoutAsync(plannedWorkoutId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get upcoming workouts
    /// </summary>
    [HttpGet("planned-workouts/upcoming")]
    [ProducesResponseType(typeof(IEnumerable<PlannedWorkoutDto>), 200)]
    public async Task<IActionResult> GetUpcomingWorkouts(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var workouts = await _trackingService.GetUpcomingWorkoutsAsync(userId, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get overdue workouts
    /// </summary>
    [HttpGet("planned-workouts/overdue")]
    [ProducesResponseType(typeof(IEnumerable<PlannedWorkoutDto>), 200)]
    public async Task<IActionResult> GetOverdueWorkouts(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var workouts = await _trackingService.GetOverdueWorkoutsAsync(userId, cancellationToken);
        return Ok(workouts);
    }

    /// <summary>
    /// Get planned workouts for a specific date
    /// </summary>
    [HttpGet("planned-workouts")]
    [ProducesResponseType(typeof(IEnumerable<PlannedWorkoutDto>), 200)]
    public async Task<IActionResult> GetPlannedWorkoutsForDate(
        [FromQuery] DateTime date,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var workouts = await _trackingService.GetPlannedWorkoutsForDateAsync(userId, date, cancellationToken);
        return Ok(workouts);
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get user tracking statistics and analytics
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(TrackingStatsDto), 200)]
    public async Task<IActionResult> GetTrackingStats(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var stats = await _trackingService.GetUserTrackingStatsAsync(userId, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get workout sessions in a date range
    /// </summary>
    [HttpGet("sessions")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutSessionListDto>), 200)]
    public async Task<IActionResult> GetWorkoutSessionsInPeriod(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var sessions = await _trackingService.GetWorkoutSessionsInPeriodAsync(userId, startDate, endDate, cancellationToken);
        return Ok(sessions);
    }

    /// <summary>
    /// Get exercise performance history
    /// </summary>
    [HttpGet("exercises/{exerciseId:guid}/performance")]
    [ProducesResponseType(typeof(ExercisePerformanceDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetExercisePerformance(
        Guid exerciseId,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var performance = await _trackingService.GetExercisePerformanceAsync(userId, exerciseId, cancellationToken);
        if (performance == null)
            return NotFound(new { Message = $"No performance data found for exercise {exerciseId}" });

        return Ok(performance);
    }

    /// <summary>
    /// Get workout frequency data
    /// </summary>
    [HttpGet("frequency")]
    [ProducesResponseType(typeof(IEnumerable<WorkoutFrequencyDto>), 200)]
    public async Task<IActionResult> GetWorkoutFrequency(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var frequency = await _trackingService.GetWorkoutFrequencyAsync(userId, startDate, endDate, cancellationToken);
        return Ok(frequency);
    }

    #endregion
}
