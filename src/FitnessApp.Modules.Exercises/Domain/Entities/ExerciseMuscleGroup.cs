namespace FitnessApp.Modules.Exercises.Domain.Entities;

public class ExerciseMuscleGroup
{
    public Guid ExerciseId { get; private set; }
    public Guid MuscleGroupId { get; private set; }
    public bool IsPrimary { get; private set; }
    
    // Navigation properties
    public Exercise Exercise { get; private set; }
    public MuscleGroup MuscleGroup { get; private set; }
    
    private ExerciseMuscleGroup() { } // For EF Core
    
    public ExerciseMuscleGroup(Exercise exercise, MuscleGroup muscleGroup, bool isPrimary = false)
    {
        Exercise = exercise ?? throw new ArgumentNullException(nameof(exercise));
        MuscleGroup = muscleGroup ?? throw new ArgumentNullException(nameof(muscleGroup));
        IsPrimary = isPrimary;
        
        ExerciseId = exercise.Id;
        MuscleGroupId = muscleGroup.Id;
    }
    
    public void SetAsPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
    }
}