using FitnessApp.Modules.Exercises.Application.Enums;

namespace FitnessApp.Modules.Exercises.Application.DTOs.Responses;
public class ExerciseResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Instructions { get; set; } = null!;
    public string? CommonMistakes { get; set; }
    public string? Tips { get; set; }
    public int? DurationInSeconds { get; set; }
    public int? CaloriesBurnedPerMinute { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<TagResponse> Tags { get; set; } = new();
    public List<ExerciseMuscleGroupReponse> MuscleGroups { get; set; } = new();
    public List<EquipmentResponse> Equipment { get; set; } = new();
    public List<MediaResourceResponse> MediaResources { get; set; } = new();
    public List<Guid> MediaAssetIds { get; set; } = new();
}