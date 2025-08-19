namespace FitnessApp.Modules.Exercises.Application.DTOs.Requests;
public record CreateMuscleGroupRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public IEnumerable<Guid>? RelatedMuscleGroupIds { get; init; } = new List<Guid>();
}