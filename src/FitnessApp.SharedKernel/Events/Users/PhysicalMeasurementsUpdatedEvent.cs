using MediatR;

namespace FitnessApp.SharedKernel.Events.Users;

/// <summary>
/// Event raised when user's physical measurements are updated
/// </summary>
public sealed record PhysicalMeasurementsUpdatedEvent : INotification
{
    public Guid UserId { get; init; }
    public decimal? Height { get; init; }
    public string? HeightUnit { get; init; }
    public decimal? Weight { get; init; }
    public string? WeightUnit { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string Source { get; init; } = "ProfileUpdate";

    public PhysicalMeasurementsUpdatedEvent(
        Guid userId,
        decimal? height,
        string? heightUnit,
        decimal? weight,
        string? weightUnit,
        DateTime updatedAt,
        string source = "ProfileUpdate")
    {
        UserId = userId;
        Height = height;
        HeightUnit = heightUnit ?? "cm";
        Weight = weight;
        WeightUnit = weightUnit ?? "kg";
        UpdatedAt = updatedAt;
        Source = source;
    }
}
