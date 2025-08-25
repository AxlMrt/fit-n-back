using FitnessApp.Modules.Workouts.Domain.Enums;

namespace FitnessApp.Modules.Workouts.Application.DTOs;

public record CreateWorkoutDto(
    string Name,
    string? Description,
    WorkoutType Type,
    DifficultyLevel Difficulty,
    int EstimatedDurationMinutes,
    EquipmentType RequiredEquipment,
    Guid? CreatedByUserId = null,
    Guid? CreatedByCoachId = null,
    List<CreateWorkoutPhaseDto>? Phases = null);

public record CreateWorkoutPhaseDto(
    WorkoutPhaseType Type,
    string Name,
    string? Description,
    int EstimatedDurationMinutes,
    List<CreateWorkoutExerciseDto>? Exercises = null);

public record CreateWorkoutExerciseDto(
    Guid ExerciseId,
    string ExerciseName,
    int? Reps = null,
    int? Sets = null,
    int? DurationSeconds = null,
    double? Weight = null,
    int? RestTimeSeconds = null,
    string? Notes = null);
