namespace FitnessApp.Modules.Exercises.Application.DTOs.Requests;
public record CreateTagRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}