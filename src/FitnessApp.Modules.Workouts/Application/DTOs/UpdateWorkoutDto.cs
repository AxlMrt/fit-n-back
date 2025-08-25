using FitnessApp.Modules.Workouts.Domain.Enums;

namespace FitnessApp.Modules.Workouts.Application.DTOs;

public record UpdateWorkoutDto(
    string? Name = null,
    string? Description = null,
    DifficultyLevel? Difficulty = null,
    int? EstimatedDurationMinutes = null,
    EquipmentType? RequiredEquipment = null,
    Guid? ImageContentId = null);

public record AddWorkoutPhaseDto(
    WorkoutPhaseType Type,
    string Name,
    string? Description,
    int EstimatedDurationMinutes);

public record UpdateWorkoutPhaseDto(
    string? Name = null,
    string? Description = null,
    int? EstimatedDurationMinutes = null);

public record AddWorkoutExerciseDto(
    Guid ExerciseId,
    string ExerciseName,
    int? Reps = null,
    int? Sets = null,
    int? DurationSeconds = null,
    double? Weight = null,
    int? RestTimeSeconds = null,
    string? Notes = null);

public record UpdateWorkoutExerciseDto(
    int? Reps = null,
    int? Sets = null,
    int? DurationSeconds = null,
    double? Weight = null,
    int? RestTimeSeconds = null,
    string? Notes = null);
