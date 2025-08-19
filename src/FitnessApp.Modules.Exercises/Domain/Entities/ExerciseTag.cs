namespace FitnessApp.Modules.Exercises.Domain.Entities;

public class ExerciseTag
{
    public Guid ExerciseId { get; private set; }
    public Guid TagId { get; private set; }
    
    // Navigation properties
    public Exercise Exercise { get; private set; }
    public Tag Tag { get; private set; }
    
    private ExerciseTag() { } // For EF Core
    
    public ExerciseTag(Exercise exercise, Tag tag)
    {
        Exercise = exercise ?? throw new ArgumentNullException(nameof(exercise));
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        
        ExerciseId = exercise.Id;
        TagId = tag.Id;
    }
}