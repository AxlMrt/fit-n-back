using FitnessApp.Modules.Workouts.Domain.Enums;

namespace FitnessApp.Modules.Workouts.Application.DTOs;

public record WorkoutQueryDto(
    WorkoutType? Type = null,
    DifficultyLevel? Difficulty = null,
    EquipmentType? Equipment = null,
    int? MaxDurationMinutes = null,
    int? MinDurationMinutes = null,
    string? SearchTerm = null,
    bool ActiveOnly = true,
    int Page = 1,
    int PageSize = 20,
    Guid? CreatedByUserId = null,
    Guid? CreatedByCoachId = null);

public record WorkoutPagedResultDto(
    IEnumerable<WorkoutSummaryDto> Workouts,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
