using System.ComponentModel.DataAnnotations;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Requests;

public sealed record CreateWorkoutDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public required string Name { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }

    [Required]
    public WorkoutType Type { get; init; }

    [Required]
    public WorkoutCategory Category { get; init; }

    [Required]
    public DifficultyLevel Difficulty { get; init; }

    // ✅ EstimatedDurationMinutes supprimé - Calculé automatiquement backend
    
    public Guid? ImageContentId { get; init; }

    public List<CreateWorkoutPhaseDto> Phases { get; init; } = [];
}

public sealed record CreateWorkoutPhaseDto
{
    [Required]
    public WorkoutPhaseType Type { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }

    // ✅ EstimatedDurationMinutes supprimé - Calculé automatiquement via exercices

    public List<CreateWorkoutExerciseDto> Exercises { get; init; } = [];
}

public sealed record CreateWorkoutExerciseDto
{
    [Required]
    public required Guid ExerciseId { get; init; }

    [Range(1, 1000)]
    public int? Reps { get; init; }

    [Range(1, 20)]
    public int? Sets { get; init; }

    [Range(1, 3600)]
    public int? DurationSeconds { get; init; }

    [Range(0.1, 500)]
    public double? Weight { get; init; }

    [Range(0.1, 100)]
    public double? Distance { get; init; }

    [Range(0, 600)]
    public int? RestTimeSeconds { get; init; }

    [StringLength(200)]
    public string? Notes { get; init; }

    public int Order { get; init; } = 0;
}

public sealed record UpdateWorkoutDto
{
    [StringLength(200, MinimumLength = 2)]
    public string? Name { get; init; }

    [StringLength(1000)]
    public string? Description { get; init; }

    public WorkoutType? Type { get; init; }

    public DifficultyLevel? Difficulty { get; init; }

    // ✅ EstimatedDurationMinutes supprimé - Recalculé automatiquement

    public Guid? ImageContentId { get; init; }
}

public sealed record AddWorkoutPhaseDto
{
    [Required]
    public WorkoutPhaseType Type { get; init; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }

    // ✅ EstimatedDurationMinutes supprimé - Calculé automatiquement
}

public sealed record UpdateWorkoutPhaseDto
{
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; init; }

    [StringLength(500)]
    public string? Description { get; init; }

    // ✅ EstimatedDurationMinutes supprimé - Recalculé automatiquement
}

public sealed record AddWorkoutExerciseDto
{
    [Required]
    public required Guid ExerciseId { get; init; }

    [Range(1, 20)]
    public int? Sets { get; init; }

    [Range(1, 1000)]
    public int? Reps { get; init; }

    [Range(0, 3600)]
    public int? DurationSeconds { get; init; }
}

public sealed record UpdateWorkoutExerciseDto
{
    [Range(1, 20)]
    public int? Sets { get; init; }

    [Range(1, 1000)]
    public int? Reps { get; init; }

    [Range(0, 3600)]
    public int? DurationSeconds { get; init; }
}

public sealed record WorkoutQueryDto
{
    public string? NameFilter { get; init; }
    public WorkoutType? Type { get; init; }
    public WorkoutCategory? Category { get; init; }
    public DifficultyLevel? Difficulty { get; init; }
    public int? MinDurationMinutes { get; init; }
    public int? MaxDurationMinutes { get; init; }
    public bool? IsActive { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string SortBy { get; init; } = "Name";
    public bool SortDescending { get; init; } = false;
}
