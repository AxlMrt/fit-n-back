using FitnessApp.Modules.Exercises.Application.Enums;

namespace FitnessApp.Modules.Exercises.Application.Dtos.Requests;

public class ExerciseSearchRequest
{
    public string? Name { get; set; }
    public List<Guid>? TagIds { get; set; }
    public List<Guid>? MuscleGroupIds { get; set; }
    public List<Guid>? EquipmentIds { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
    public int? MaxDurationInSeconds { get; set; }
    public bool? RequiresEquipment { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public string? SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; } = false;
}


