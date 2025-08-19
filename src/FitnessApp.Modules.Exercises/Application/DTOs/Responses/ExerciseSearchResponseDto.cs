
using FitnessApp.Modules.Exercises.Application.Enums;

namespace FitnessApp.Modules.Exercises.Application.Dtos.Responses;

public class ExerciseSearchResponse
{
    public List<ExerciseListItemResponse> Exercises { get; set; } = new();
    public int TotalCount { get; set; }
}

public class ExerciseListItemResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DifficultyLevel Difficulty { get; set; }
    public int? DurationInSeconds { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public List<string> MuscleGroupNames { get; set; } = new();
    public List<string> EquipmentNames { get; set; } = new();
    public List<string> TagNames { get; set; } = new();
}