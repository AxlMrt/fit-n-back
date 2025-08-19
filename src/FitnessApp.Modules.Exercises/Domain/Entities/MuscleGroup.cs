namespace FitnessApp.Modules.Exercises.Domain.Entities;
public class MuscleGroup
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string BodyPart { get; private set; }
    
    // Navigation property
    public ICollection<ExerciseMuscleGroup> Exercises { get; private set; } = new List<ExerciseMuscleGroup>();
    
    private MuscleGroup() { } // For EF Core
    
    public MuscleGroup(string name, string description, string bodyPart)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        BodyPart = bodyPart;
        
        Validate();
    }
    
    public void Update(string name, string description, string bodyPart)
    {
        Name = name;
        Description = description;
        BodyPart = bodyPart;
        
        Validate();
    }
    
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Muscle group name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(BodyPart))
            throw new ArgumentException("Body part cannot be empty");
    }
}