namespace FitnessApp.Modules.Content.Infrastructure.Workers;

public record TranscodeRequest(Guid AssetId, string Key, string TargetFormat);
