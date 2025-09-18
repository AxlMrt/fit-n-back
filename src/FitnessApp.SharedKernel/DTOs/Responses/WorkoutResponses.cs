using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Responses;

public sealed record WorkoutDto(
    Guid Id,
    string Name,
    string? Description,
    WorkoutType Type,
    WorkoutCategory Category,
    DifficultyLevel Difficulty,
    int EstimatedDurationMinutes,
    Guid? ImageContentId,
    bool IsActive,
    int PhaseCount,
    int TotalExercises,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<WorkoutPhaseDto> Phases
);

public sealed record WorkoutListDto(
    Guid Id,
    string Name,
    string? Description,
    WorkoutType Type,
    WorkoutCategory Category,
    DifficultyLevel Difficulty,
    int EstimatedDurationMinutes,
    Guid? ImageContentId,
    bool IsActive,
    int PhaseCount,
    int TotalExercises,
    DateTime CreatedAt
);

public sealed record WorkoutPhaseDto(
    Guid Id,
    WorkoutPhaseType Type,
    string Name,
    string? Description,
    int EstimatedDurationMinutes,
    int Order,
    List<WorkoutExerciseDto> Exercises
);

public sealed record WorkoutExerciseDto(
    Guid Id,
    Guid ExerciseId,
    int? Reps,
    int? Sets,
    int? DurationSeconds,
    double? Weight,
    double? Distance,
    int? RestSeconds,
    string? Notes,
    int Order
);
