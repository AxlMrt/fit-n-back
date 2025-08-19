using FitnessApp.Modules.Exercises.Application.Enums;

namespace FitnessApp.Modules.Exercises.Application.Dtos.Requests;

public class UpdateExerciseRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Instructions { get; set; }
    public string? CommonMistakes { get; set; }
    public string? Tips { get; set; }
    public int? DurationInSeconds { get; set; }
    public int? CaloriesBurnedPerMinute { get; set; }
    public DifficultyLevel? Difficulty { get; set; }
}