namespace FitnessApp.Modules.Exercises.Domain.Entities;
public class Equipment
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    //public string Category { get; private set; }
    
    // Navigation property
    public ICollection<ExerciseEquipment> Exercises { get; private set; } = new List<ExerciseEquipment>();
    
    private Equipment() { } 
    
    public Equipment(string name, string description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        
        Validate();
    }
    
    public void Update(string name, string description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
        
        Validate();
    }
    
    // public void SetCategory(string category)
    // {
    //     if (string.IsNullOrWhiteSpace(category))
    //         throw new ArgumentException("Category cannot be empty");
            
    //     Category = category;
    //     UpdatedAt = DateTime.UtcNow;
    // }
    
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Equipment name cannot be empty");
    }
}