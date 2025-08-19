namespace FitnessApp.Modules.Exercises.Domain.Entities;

public class ExerciseCategory
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Icon { get; private set; }
    
    // Navigation property
    public ICollection<Exercise> Exercises { get; private set; } = new List<Exercise>();
    
    private ExerciseCategory() { } // For EF Core
    
    public ExerciseCategory(string name, string description, string icon = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Icon = icon;
        
        Validate();
    }
    
    public void Update(string name, string description, string icon)
    {
        Name = name;
        Description = description;
        Icon = icon;
        
        Validate();
    }
    
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Category name cannot be empty");
    }
}