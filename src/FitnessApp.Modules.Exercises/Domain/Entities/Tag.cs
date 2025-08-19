namespace FitnessApp.Modules.Exercises.Domain.Entities;
public class Tag
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    
    // Navigation property
    public ICollection<ExerciseTag> ExerciseTags { get; private set; } = new List<ExerciseTag>();
    
    private Tag() { }
    
    public Tag(string name, string description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        
        Validate();
    }
    
    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
        
        Validate();
    }
    
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Tag name cannot be empty");
    }
}