using FitnessApp.Modules.Workouts.Domain.Enums;

namespace FitnessApp.Modules.Workouts.Application.DTOs;

public record WorkoutDto(
    Guid Id,
    string Name,
    string? Description,
    WorkoutType Type,
    DifficultyLevel Difficulty,
    int EstimatedDurationMinutes,
    EquipmentType RequiredEquipment,
    bool IsActive,
    Guid? ImageContentId,
    Guid? CreatedByUserId,
    Guid? CreatedByCoachId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<WorkoutPhaseDto> Phases);

public record WorkoutPhaseDto(
    Guid Id,
    WorkoutPhaseType Type,
    string Name,
    string? Description,
    int EstimatedDurationMinutes,
    int Order,
    List<WorkoutExerciseDto> Exercises);

public record WorkoutExerciseDto(
    Guid Id,
    Guid ExerciseId,
    string ExerciseName,
    int? Reps,
    int? Sets,
    int? DurationSeconds,
    double? Weight,
    int? RestTimeSeconds,
    string? Notes,
    int Order);

public record WorkoutSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    WorkoutType Type,
    DifficultyLevel Difficulty,
    int EstimatedDurationMinutes,
    EquipmentType RequiredEquipment,
    bool IsActive,
    Guid? ImageContentId,
    int PhaseCount,
    int ExerciseCount,
    DateTime CreatedAt);
