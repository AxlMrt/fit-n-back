namespace FitnessApp.Modules.Exercises.Domain.Entities;

public class ExerciseVariation
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ModificationType { get; private set; } // "Easier", "Harder", "Alternative"
    
    // Foreign key
    public Guid BaseExerciseId { get; private set; }
    public Guid VariationExerciseId { get; private set; }
    
    // Navigation properties
    public Exercise BaseExercise { get; private set; }
    public Exercise VariationExercise { get; private set; }
    
    private ExerciseVariation() { } // For EF Core
    
    public ExerciseVariation(string name, string description, string modificationType, 
                            Exercise baseExercise, Exercise variationExercise)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        ModificationType = modificationType;
        BaseExercise = baseExercise ?? throw new ArgumentNullException(nameof(baseExercise));
        VariationExercise = variationExercise ?? throw new ArgumentNullException(nameof(variationExercise));
        
        BaseExerciseId = baseExercise.Id;
        VariationExerciseId = variationExercise.Id;
        
        Validate();
    }
    
    public void Update(string name, string description, string modificationType)
    {
        Name = name;
        Description = description;
        ModificationType = modificationType;
        
        Validate();
    }
    
    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Variation name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(ModificationType))
            throw new ArgumentException("Modification type cannot be empty");
            
        if (BaseExerciseId == VariationExerciseId)
            throw new ArgumentException("Base exercise and variation exercise cannot be the same");
    }
}