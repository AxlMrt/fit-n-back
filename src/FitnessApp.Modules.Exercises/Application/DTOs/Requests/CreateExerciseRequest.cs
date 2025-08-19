using FitnessApp.Modules.Exercises.Application.Enums;

namespace FitnessApp.Modules.Exercises.Application.Dtos.Requests;

public class CreateExerciseRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Instructions { get; set; } = null!;
    public string? CommonMistakes { get; set; }
    public string? Tips { get; set; }
    public int? DurationInSeconds { get; set; }
    public int? CaloriesBurnedPerMinute { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public List<Guid>? TagIds { get; set; }
    public List<MuscleGroupAssignment>? MuscleGroups { get; set; }
    public List<Guid>? EquipmentIds { get; set; }
}

public class MuscleGroupAssignment
{
    public Guid MuscleGroupId { get; set; }
    public bool IsPrimary { get; set; }
}