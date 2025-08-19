namespace FitnessApp.Modules.Exercises.Application.DTOs.Requests;
public record CreateEquipmentRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
}