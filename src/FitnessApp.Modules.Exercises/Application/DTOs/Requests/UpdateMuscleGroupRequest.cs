namespace FitnessApp.Modules.Exercises.Application.DTOs.Requests;
public record UpdateMuscleGroupRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public IEnumerable<Guid>? RelatedMuscleGroupIds { get; init; }
}