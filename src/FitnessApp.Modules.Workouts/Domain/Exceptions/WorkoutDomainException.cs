using FitnessApp.SharedKernel.Exceptions;

namespace FitnessApp.Modules.Workouts.Domain.Exceptions;

/// <summary>
/// Base exception for Workout domain errors
/// </summary>
public sealed class WorkoutDomainException : DomainException
{
    public WorkoutDomainException(string errorCode, string message) 
        : base("Workouts", errorCode, message)
    {
    }

    public WorkoutDomainException(string errorCode, string message, Exception innerException) 
        : base("Workouts", errorCode, message, innerException)
    {
    }

    // Factory methods for common workout domain errors
    public static WorkoutDomainException InvalidName(string name) =>
        new("INVALID_NAME", $"Workout name '{name}' is invalid");

    public static WorkoutDomainException InvalidDuration(int minutes) =>
        new("INVALID_DURATION", $"Duration {minutes} minutes is invalid. Must be between 1 and 300 minutes");

    public static WorkoutDomainException PhaseNotFound(string phaseType) =>
        new("PHASE_NOT_FOUND", $"Phase of type {phaseType} not found");

    public static WorkoutDomainException DuplicatePhase(string phaseType) =>
        new("DUPLICATE_PHASE", $"Phase of type {phaseType} already exists");

    public static WorkoutDomainException UnauthorizedAccess(string action) =>
        new("UNAUTHORIZED_ACCESS", $"User cannot {action} this workout");

    // Factory methods for common validation errors
    public static WorkoutDomainException NameRequired() =>
        new("NAME_REQUIRED", "Workout name is required");

    public static WorkoutDomainException NameTooLong(int maxLength) =>
        new("NAME_TOO_LONG", $"Workout name cannot exceed {maxLength} characters");

    public static WorkoutDomainException InvalidDurationRange(int minMinutes, int maxMinutes) =>
        new("INVALID_DURATION_RANGE", $"Duration must be between {minMinutes} and {maxMinutes} minutes");

    public static WorkoutDomainException PhaseNameRequired() =>
        new("PHASE_NAME_REQUIRED", "Phase name is required");

    public static WorkoutDomainException PhaseNameTooLong(int maxLength) =>
        new("PHASE_NAME_TOO_LONG", $"Phase name cannot exceed {maxLength} characters");

    public static WorkoutDomainException InvalidPhaseOrder() =>
        new("INVALID_PHASE_ORDER", "Order must be at least 1");

    public static WorkoutDomainException InvalidPhaseDuration(int minMinutes, int maxMinutes) =>
        new("INVALID_PHASE_DURATION", $"Phase duration must be between {minMinutes} and {maxMinutes} minutes");

    public static WorkoutDomainException ExerciseAlreadyExists(Guid exerciseId) =>
        new("EXERCISE_ALREADY_EXISTS", $"Exercise {exerciseId} already exists in this phase");

    public static WorkoutDomainException ExerciseNotFound(Guid exerciseId) =>
        new("EXERCISE_NOT_FOUND", $"Exercise {exerciseId} not found in this phase");

    public static WorkoutDomainException InvalidExerciseOrder(int maxOrder) =>
        new("INVALID_EXERCISE_ORDER", $"Order must be between 1 and {maxOrder}");

    public static WorkoutDomainException WorkoutNotFound(Guid workoutId) =>
        new("WORKOUT_NOT_FOUND", $"Workout with ID {workoutId} not found");

    public static WorkoutDomainException PhaseNotFoundById(Guid phaseId) =>
        new("PHASE_NOT_FOUND_BY_ID", $"Phase with ID {phaseId} not found");

    // Factory methods for exercise validation
    public static WorkoutDomainException EmptyExerciseId() =>
        new("EMPTY_EXERCISE_ID", "Exercise ID cannot be empty");

    public static WorkoutDomainException InvalidOrder() =>
        new("INVALID_ORDER", "Order must be at least 1");

    public static WorkoutDomainException NegativeRestTime() =>
        new("NEGATIVE_REST_TIME", "Rest time cannot be negative");

    public static WorkoutDomainException NoExerciseParameters() =>
        new("NO_EXERCISE_PARAMETERS", "At least one exercise parameter (sets, reps, duration, or distance) must be specified");

    public static WorkoutDomainException InvalidSetsReps() =>
        new("INVALID_SETS_REPS", "Sets and reps must be greater than 0 when specified");

    public static WorkoutDomainException InvalidDurationValue() =>
        new("INVALID_DURATION_VALUE", "Duration must be greater than 0 when specified");

    public static WorkoutDomainException InvalidDistance() =>
        new("INVALID_DISTANCE", "Distance must be greater than 0 when specified");

    public static WorkoutDomainException InvalidWeight() =>
        new("INVALID_WEIGHT", "Weight must be greater than 0 when specified");

    public static WorkoutDomainException InvalidSets() =>
        new("INVALID_SETS", "Sets must be greater than 0");

    public static WorkoutDomainException InvalidReps() =>
        new("INVALID_REPS", "Reps must be greater than 0");

    public static WorkoutDomainException InvalidPhaseOrderRange(int maxOrder) =>
        new("INVALID_PHASE_ORDER_RANGE", $"Order must be between 1 and {maxOrder}");

    public static WorkoutDomainException ExerciseNotFoundInWorkout(Guid exerciseId) =>
        new("EXERCISE_NOT_FOUND_IN_WORKOUT", $"Exercise with ID {exerciseId} not found");

    public static WorkoutDomainException OnlyUserCreatedWorkoutsCanBeModified() =>
        new("ONLY_USER_CREATED_WORKOUTS", "Only user-created workouts can be modified by users");

    public static WorkoutDomainException UserCanOnlyModifyOwnWorkouts() =>
        new("USER_CAN_ONLY_MODIFY_OWN", "User can only modify workouts they created");

    public static WorkoutDomainException PredefinedWorkoutsCannotBeDeleted() =>
        new("PREDEFINED_WORKOUTS_CANNOT_BE_DELETED", "User cannot delete predefined workouts");
}
