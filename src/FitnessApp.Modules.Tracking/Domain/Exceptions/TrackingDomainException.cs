using FitnessApp.SharedKernel.Exceptions;

namespace FitnessApp.Modules.Tracking.Domain.Exceptions;

/// <summary>
/// Domain exception specific to tracking operations
/// </summary>
public sealed class TrackingDomainException : DomainException
{
    public TrackingDomainException(string errorCode, string message) 
        : base("Tracking", errorCode, message)
    {
    }
    
    public TrackingDomainException(string errorCode, string message, Exception innerException) 
        : base("Tracking", errorCode, message, innerException)
    {
    }

    // Factory methods for common tracking domain errors
    public static TrackingDomainException InvalidSessionStatus(string currentStatus, string attemptedAction) =>
        new("INVALID_SESSION_STATUS", $"Cannot {attemptedAction} session with status {currentStatus}");

    public static TrackingDomainException NegativeMetricValue() =>
        new("NEGATIVE_METRIC_VALUE", "Metric value cannot be negative");

    public static TrackingDomainException EmptyUserId() =>
        new("EMPTY_USER_ID", "User ID cannot be empty");

    public static TrackingDomainException SessionAlreadyStarted() =>
        new("SESSION_ALREADY_STARTED", "Workout session has already been started");

    public static TrackingDomainException SessionNotStarted() =>
        new("SESSION_NOT_STARTED", "Workout session has not been started yet");
    
    // Planned workout factory methods
    public static TrackingDomainException UserIdCannotBeEmpty() =>
        new("USER_ID_CANNOT_BE_EMPTY", "User ID cannot be empty");
    
    public static TrackingDomainException WorkoutIdCannotBeEmpty() =>
        new("WORKOUT_ID_CANNOT_BE_EMPTY", "Workout ID cannot be empty");
    
    public static TrackingDomainException CannotStartPlannedWorkout(string status) =>
        new("CANNOT_START_PLANNED_WORKOUT", $"Cannot start planned workout with status {status}");
    
    public static TrackingDomainException WorkoutSessionIdCannotBeEmpty() =>
        new("WORKOUT_SESSION_ID_CANNOT_BE_EMPTY", "Workout session ID cannot be empty");
    
    public static TrackingDomainException CannotCompletePlannedWorkout(string status) =>
        new("CANNOT_COMPLETE_PLANNED_WORKOUT", $"Cannot complete planned workout with status {status}");
    
    public static TrackingDomainException CannotCancelPlannedWorkout(string status) =>
        new("CANNOT_CANCEL_PLANNED_WORKOUT", $"Cannot cancel planned workout with status {status}");
    
    public static TrackingDomainException CannotAbandonPlannedWorkout(string status) =>
        new("CANNOT_ABANDON_PLANNED_WORKOUT", $"Cannot abandon planned workout with status {status}");
    
    public static TrackingDomainException CannotRescheduleWorkout(string status) =>
        new("CANNOT_RESCHEDULE_WORKOUT", $"Cannot reschedule workout with status {status}");
    
    // Workout session factory methods
    public static TrackingDomainException CannotStartSession(string status) =>
        new("CANNOT_START_SESSION", $"Cannot start session with status {status}");
    
    public static TrackingDomainException CannotCompleteSession(string status) =>
        new("CANNOT_COMPLETE_SESSION", $"Cannot complete session with status {status}");
    
    public static TrackingDomainException CannotCompleteSessionWithoutStartTime() =>
        new("CANNOT_COMPLETE_SESSION_WITHOUT_START_TIME", "Cannot complete session without start time");
    
    public static TrackingDomainException CannotAbandonSession(string status) =>
        new("CANNOT_ABANDON_SESSION", $"Cannot abandon session with status {status}");
    
    public static TrackingDomainException CannotCancelSession(string status) =>
        new("CANNOT_CANCEL_SESSION", $"Cannot cancel session with status {status}");
    
    public static TrackingDomainException CanOnlyAddExercisesToActiveSession() =>
        new("CAN_ONLY_ADD_EXERCISES_TO_ACTIVE_SESSION", "Can only add exercises to sessions in progress");
    
    public static TrackingDomainException ExerciseIdCannotBeEmpty() =>
        new("EXERCISE_ID_CANNOT_BE_EMPTY", "Exercise ID cannot be empty");
    
    public static TrackingDomainException ExerciseNameRequired() =>
        new("EXERCISE_NAME_REQUIRED", "Exercise name is required");
    
    public static TrackingDomainException CanOnlyModifyExercisesInActiveSession() =>
        new("CAN_ONLY_MODIFY_EXERCISES_IN_ACTIVE_SESSION", "Can only modify exercises in sessions in progress");
    
    public static TrackingDomainException ExerciseNotFoundInSession(Guid exerciseId) =>
        new("EXERCISE_NOT_FOUND_IN_SESSION", $"Exercise {exerciseId} not found in session");
    
    public static TrackingDomainException CanOnlyUpdatePlannedDateForPlannedSessions() =>
        new("CAN_ONLY_UPDATE_PLANNED_DATE_FOR_PLANNED_SESSIONS", "Can only update planned date for planned sessions");
    
    public static TrackingDomainException CaloriesCannotBeNegative() =>
        new("CALORIES_CANNOT_BE_NEGATIVE", "Calories cannot be negative");
    
    // Workout session exercise factory methods
    public static TrackingDomainException OrderMustBeAtLeastOne() =>
        new("ORDER_MUST_BE_AT_LEAST_ONE", "Order must be at least 1");
    
    public static TrackingDomainException SetNotFound(int setNumber) =>
        new("SET_NOT_FOUND", $"Set number {setNumber} not found");
    
    public static TrackingDomainException PerformanceScoreOutOfRange() =>
        new("PERFORMANCE_SCORE_OUT_OF_RANGE", "Performance score must be between 0 and 100");
    
    // Tracking service factory methods
    public static TrackingDomainException UserAlreadyHasActiveWorkoutSession() =>
        new("USER_ALREADY_HAS_ACTIVE_WORKOUT_SESSION", "User already has an active workout session");
    
    public static TrackingDomainException WorkoutAlreadyScheduled(DateTime scheduledDate) =>
        new("WORKOUT_ALREADY_SCHEDULED", $"Workout is already scheduled for {scheduledDate:yyyy-MM-dd}");
    
    // Not found factory methods
    public static TrackingDomainException MetricNotFound(Guid metricId) =>
        new("METRIC_NOT_FOUND", $"Metric with ID {metricId} not found");
    
    public static TrackingDomainException WorkoutSessionNotFound(Guid sessionId) =>
        new("WORKOUT_SESSION_NOT_FOUND", $"Workout session with ID {sessionId} not found");
}
