namespace FitnessApp.Modules.Exercises.Domain.Entities;

public class ExerciseEquipment
{
    public Guid ExerciseId { get; private set; }
    public Guid EquipmentId { get; private set; }
    
    // Navigation properties
    public Exercise Exercise { get; private set; }
    public Equipment Equipment { get; private set; }
    
    private ExerciseEquipment() { } // For EF Core
    
    public ExerciseEquipment(Exercise exercise, Equipment equipment)
    {
        Exercise = exercise ?? throw new ArgumentNullException(nameof(exercise));
        Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        
        ExerciseId = exercise.Id;
        EquipmentId = equipment.Id;
    }
}