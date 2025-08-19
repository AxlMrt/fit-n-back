namespace FitnessApp.Modules.Exercises.Domain.Entities;
public class Exercise
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string Instructions { get; private set; }
    public string CommonMistakes { get; private set; }
    public string DifficultyLevel { get; private set; }
    public int? EstimatedCaloriesBurn { get; private set; }
    public bool IsBodyweightExercise { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    
    // Navigation properties
    public ICollection<ExerciseTag> ExerciseTags { get; private set; } = new List<ExerciseTag>();
    public ICollection<ExerciseMuscleGroup> TargetMuscleGroups { get; private set; } = new List<ExerciseMuscleGroup>();
    public ICollection<ExerciseEquipment> RequiredEquipment { get; private set; } = new List<ExerciseEquipment>();
    public ICollection<Guid> MediaAssetIds { get; private set; } = new List<Guid>();

    private Exercise() { } // For EF Core

    public Exercise(
        string name, 
        string description, 
        string instructions, 
        string difficultyLevel,
        bool isBodyweightExercise)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Instructions = instructions;
        DifficultyLevel = difficultyLevel;
        IsBodyweightExercise = isBodyweightExercise;
        CreatedAt = DateTime.UtcNow;
        
        Validate();
    }

    public void Update(
        string name, 
        string description, 
        string instructions, 
        string commonMistakes,
        string difficultyLevel,
        int? estimatedCaloriesBurn,
        bool isBodyweightExercise)
    {
        Name = name;
        Description = description;
        Instructions = instructions;
        CommonMistakes = commonMistakes;
        DifficultyLevel = difficultyLevel;
        EstimatedCaloriesBurn = estimatedCaloriesBurn;
        IsBodyweightExercise = isBodyweightExercise;
        UpdatedAt = DateTime.UtcNow;
        
        Validate();
    }

    public void AddTag(Tag tag)
    {
        if (!ExerciseTags.Any(et => et.TagId == tag.Id))
        {
            ExerciseTags.Add(new ExerciseTag(this, tag));
        }
    }

    public void AddMuscleGroup(MuscleGroup muscleGroup, bool isPrimary = false)
    {
        if (!TargetMuscleGroups.Any(tmg => tmg.MuscleGroupId == muscleGroup.Id))
        {
            TargetMuscleGroups.Add(new ExerciseMuscleGroup(this, muscleGroup, isPrimary));
        }
    }

    public void AddEquipment(Equipment equipment)
    {
        if (!RequiredEquipment.Any(re => re.EquipmentId == equipment.Id))
        {
            RequiredEquipment.Add(new ExerciseEquipment(this, equipment));
        }
    }

    public void AddMediaAsset(Guid assetId)
    {
        if (!MediaAssetIds.Contains(assetId)) MediaAssetIds.Add(assetId);
    }

    private void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Exercise name cannot be empty");
        
        if (string.IsNullOrWhiteSpace(Description))
            throw new ArgumentException("Exercise description cannot be empty");
        
        if (string.IsNullOrWhiteSpace(Instructions))
            throw new ArgumentException("Exercise instructions cannot be empty");
    }
}