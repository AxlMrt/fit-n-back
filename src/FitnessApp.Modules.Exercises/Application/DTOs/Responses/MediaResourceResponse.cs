using FitnessApp.Modules.Exercises.Application.Enums;

namespace FitnessApp.Modules.Exercises.Application.DTOs.Responses;
public class MediaResourceResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public MediaType Type { get; set; }
    public string Url { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public int? DurationInSeconds { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}