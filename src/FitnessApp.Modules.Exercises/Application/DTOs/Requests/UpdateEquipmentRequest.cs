namespace FitnessApp.Modules.Exercises.Application.DTOs.Requests;
public record UpdateEquipmentRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
}
