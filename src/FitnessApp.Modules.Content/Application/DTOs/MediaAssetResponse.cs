namespace FitnessApp.Modules.Content.Application.DTOs
{
    public record MediaAssetResponse(
        Guid Id,
        string Url,
        string Type,
        string? Description,
        DateTime CreatedAt,
        string? ContentType
    );
}
