namespace FitnessApp.Modules.Exercises.Application.DTOs.Requests;

public record UpdateTagRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
}