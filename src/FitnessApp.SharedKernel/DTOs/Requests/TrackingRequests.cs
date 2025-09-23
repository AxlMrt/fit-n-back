using System.ComponentModel.DataAnnotations;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Requests;

public sealed record StartWorkoutSessionRequest
{
    [Required]
    public required Guid WorkoutId { get; init; }

    public Guid? ProgramId { get; init; }

    public bool IsFromProgram { get; init; } = false;
}

public sealed record CompleteWorkoutSessionRequest
{
    [Required]
    [Range(1, 6)]
    public required PerceivedDifficulty PerceivedDifficulty { get; init; }

    [StringLength(1000)]
    public string? Notes { get; init; }
}

public sealed record AddExercisePerformanceRequest
{
    [Required]
    public required Guid ExerciseId { get; init; }

    [Required]
    public required ExerciseMetricType MetricType { get; init; }

    [Range(1, 1000)]
    public int? Repetitions { get; init; }

    [Range(0, 1000)]
    public double? Weight { get; init; }

    [Range(1, 86400)] // Max 24 hours in seconds
    public int? DurationSeconds { get; init; }

    [Range(0, 100000)] // Max 100km in meters
    public double? Distance { get; init; }
}

public sealed record RecordUserMetricRequest
{
    [Required]
    public required UserMetricType MetricType { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required double Value { get; init; }

    public DateTime? RecordedAt { get; init; }

    [StringLength(500)]
    public string? Notes { get; init; }

    [StringLength(20)]
    public string? Unit { get; init; }
}

public sealed record ScheduleWorkoutRequest
{
    [Required]
    public required Guid WorkoutId { get; init; }

    [Required]
    public required DateTime ScheduledDate { get; init; }

    public bool IsFromProgram { get; init; } = false;

    public Guid? ProgramId { get; init; }
}

public sealed record UpdateExercisePerformanceRequest
{
    [Range(1, 1000)]
    public int? Repetitions { get; init; }

    [Range(0, 1000)]
    public double? Weight { get; init; }

    [Range(1, 86400)]
    public int? DurationSeconds { get; init; }

    [Range(0, 100000)]
    public double? Distance { get; init; }
}

public sealed record RescheduleWorkoutRequest
{
    [Required]
    public required DateTime NewScheduledDate { get; init; }
}

public sealed record AddSetRequest
{
    [Range(1, 1000)]
    public int? Repetitions { get; init; }

    [Range(0, 1000)]
    public double? Weight { get; init; }

    [Range(1, 86400)]
    public int? DurationSeconds { get; init; }

    [Range(0, 100000)]
    public double? Distance { get; init; }

    [Range(0, 7200)] // Max 2h rest time
    public int? RestTimeSeconds { get; init; }
}