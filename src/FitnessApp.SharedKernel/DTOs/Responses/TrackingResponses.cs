using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Responses;

public sealed record WorkoutSessionDto(
    Guid Id,
    Guid UserId,
    Guid WorkoutId,
    DateTime? PlannedDate,
    DateTime? StartTime,
    DateTime? EndTime,
    WorkoutSessionStatus Status,
    int? TotalDurationSeconds,
    int? CaloriesEstimated,
    PerceivedDifficulty? PerceivedDifficulty,
    string? Notes,
    bool IsFromProgram,
    Guid? ProgramId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<WorkoutSessionExerciseDto> Exercises
);

public sealed record WorkoutSessionListDto(
    Guid Id,
    Guid UserId,
    Guid WorkoutId,
    DateTime? PlannedDate,
    DateTime? StartTime,
    DateTime? EndTime,
    WorkoutSessionStatus Status,
    int? TotalDurationSeconds,
    int? CaloriesEstimated,
    PerceivedDifficulty? PerceivedDifficulty,
    bool IsFromProgram,
    Guid? ProgramId,
    DateTime CreatedAt,
    int ExerciseCount
);

public sealed record WorkoutSessionExerciseDto(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    ExerciseMetricType MetricType,
    int Order,
    List<WorkoutSessionSetDto> Sets,
    DateTime PerformedAt,
    string PerformanceDisplay
);

public sealed record WorkoutSessionSetDto(
    Guid Id,
    int SetNumber,
    int? Repetitions,
    double? Weight,
    int? DurationSeconds,
    double? Distance,
    int? RestTimeSeconds,
    DateTime PerformedAt,
    string DisplayText
);

public sealed record UserMetricDto(
    Guid Id,
    Guid UserId,
    UserMetricType MetricType,
    double Value,
    string Unit,
    DateTime RecordedAt,
    string? Notes,
    DateTime CreatedAt,
    string DisplayValue
);

public sealed record PlannedWorkoutDto(
    Guid Id,
    Guid UserId,
    Guid WorkoutId,
    DateTime ScheduledDate,
    WorkoutSessionStatus Status,
    bool IsFromProgram,
    Guid? ProgramId,
    Guid? WorkoutSessionId,
    DateTime CreatedAt,
    bool IsOverdue,
    int DaysUntilScheduled
);

public sealed record TrackingStatsDto(
    int TotalWorkoutsCompleted,
    int TotalWorkoutTime,
    int AverageWorkoutDuration,
    int TotalCaloriesBurned,
    int WorkoutsThisWeek,
    int WorkoutsThisMonth,
    double? AveragePerceivedDifficulty,
    DateTime? LastWorkoutDate,
    int CurrentStreak,
    int LongestStreak,
    List<WorkoutFrequencyDto> WeeklyFrequency,
    List<MetricTrendDto> MetricTrends
);

public sealed record WorkoutFrequencyDto(
    DateTime Date,
    int WorkoutCount,
    int TotalDuration
);

public sealed record MetricTrendDto(
    UserMetricType MetricType,
    double CurrentValue,
    double? PreviousValue,
    double? PercentageChange,
    string Trend, // "up", "down", "stable"
    DateTime LastRecorded
);

public sealed record ExercisePerformanceDto(
    Guid ExerciseId,
    string ExerciseName,
    ExerciseMetricType MetricType,
    double? BestValue,
    double? AverageValue,
    int TimesPerformed,
    DateTime? LastPerformed,
    List<PerformanceHistoryDto> History
);

public sealed record PerformanceHistoryDto(
    DateTime Date,
    double Value,
    string? Notes
);