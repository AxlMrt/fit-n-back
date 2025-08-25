using FitnessApp.Modules.Exercises.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Modules.Exercises.Application.DTOs
{
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

    public sealed record CreateExerciseDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Name { get; init; }

        [StringLength(1000)]
        public string? Description { get; init; }

        [Required]
        public ExerciseType Type { get; init; }

        public List<string> MuscleGroups { get; init; } = new();

        public DifficultyLevel Difficulty { get; init; } = DifficultyLevel.Unknown;

        public List<string> Equipment { get; init; } = new();

        [StringLength(2000)]
        public string? Instructions { get; init; }

        public Guid? ImageContentId { get; init; }
        public Guid? VideoContentId { get; init; }
    }

    public sealed record UpdateExerciseDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; init; }

        [StringLength(1000)]
        public string? Description { get; init; }

        public ExerciseType? Type { get; init; }

        public List<string>? MuscleGroups { get; init; }

        public DifficultyLevel? Difficulty { get; init; }

        public List<string>? Equipment { get; init; }

        [StringLength(2000)]
        public string? Instructions { get; init; }

        public Guid? ImageContentId { get; init; }
        public Guid? VideoContentId { get; init; }
    }

    public sealed record ExerciseQueryDto
    {
        public string? NameFilter { get; init; }
        public ExerciseType? Type { get; init; }
        public DifficultyLevel? Difficulty { get; init; }
        public List<string>? MuscleGroups { get; init; }
        public bool? RequiresEquipment { get; init; }
        public bool? IsActive { get; init; }
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public string SortBy { get; init; } = "Name";
        public bool SortDescending { get; init; } = false;
    }

    public sealed record ExerciseListDto(
        Guid Id,
        string Name,
        ExerciseType Type,
        DifficultyLevel Difficulty,
        List<string> MuscleGroups,
        bool RequiresEquipment,
        bool IsActive
    );

    public sealed record PagedResultDto<T>(
        List<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    ) where T : class;
}