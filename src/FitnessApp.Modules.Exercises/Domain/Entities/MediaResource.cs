namespace FitnessApp.Modules.Exercises.Domain.Entities;
public class MediaResource
{
    public Guid Id { get; private set; }
    public string Url { get; private set; }
    public string Type { get; private set; } // "video", "image", "gif"
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Foreign key
    public Guid ExerciseId { get; private set; }
    
    // Navigation property
    public Exercise Exercise { get; private set; }
    
    // New properties for file storage
    public string? FilePath { get; set; }
    public string? ContentType { get; set; }

    private MediaResource() { } // For EF Core
    
    public MediaResource(string url, string type, string description, Exercise exercise)
    {
        Id = Guid.NewGuid();
        Url = url;
        Type = type;
        Description = description;
        Exercise = exercise;
        ExerciseId = exercise.Id;
        CreatedAt = DateTime.UtcNow;
        
        Validate();
    }
    
    public void Update(string url, string type, string description)
    {
        Url = url;
        Type = type;
        Description = description;
        
        Validate();
    }
    
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Url))
            throw new ArgumentException("Media URL cannot be empty");
        
        if (string.IsNullOrWhiteSpace(Type))
            throw new ArgumentException("Media type cannot be empty");
    }
}