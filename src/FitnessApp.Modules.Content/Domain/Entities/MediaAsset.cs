namespace FitnessApp.Modules.Content.Domain.Entities;

public class MediaAsset
{
    public Guid Id { get; private set; }
    public string Key { get; private set; } // object key in Minio/S3
    public string Url { get; private set; }
    public string Type { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? ContentType { get; private set; }

    private MediaAsset() { }

    public MediaAsset(string key, string url, string type, string? description, string? contentType)
    {
        Id = Guid.NewGuid();
        Key = key;
        Url = url;
        Type = type;
        Description = description;
        ContentType = contentType;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }
}
