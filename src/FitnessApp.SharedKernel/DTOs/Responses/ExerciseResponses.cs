using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.SharedKernel.DTOs.Responses;
public sealed record ExerciseDto(
    Guid Id,
    string Name,
    string? Description,
    ExerciseType Type,
    List<string> MuscleGroups,
    Guid? ImageContentId,
    Guid? VideoContentId,
    DifficultyLevel Difficulty,
    List<string> Equipment,
    string? Instructions,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public sealed record ExerciseListDto(
    Guid Id,
    string Name,
    ExerciseType Type,
    DifficultyLevel Difficulty,
    List<string> MuscleGroups,
    bool RequiresEquipment,
    bool IsActive
);