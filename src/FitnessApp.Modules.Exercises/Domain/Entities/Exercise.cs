using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;
using FitnessApp.Modules.Exercises.Domain.Exceptions;

namespace FitnessApp.Modules.Exercises.Domain.Entities
{
    public class Exercise
    {
        private Exercise() { } // For EF Core

        public Exercise(string name, ExerciseType type, DifficultyLevel difficulty, MuscleGroup muscleGroups, Equipment equipment)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ExerciseDomainException("Exercise name is required");
            
            if (name.Length > 100)
                throw new ExerciseDomainException("Exercise name cannot exceed 100 characters");
                
            Id = Guid.NewGuid();
            Name = name.Trim();
            Type = type;
            Difficulty = difficulty;
            MuscleGroups = muscleGroups;
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            CreatedAt = DateTime.UtcNow;
            // UpdatedAt remains null until first update
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public ExerciseType Type { get; private set; }
        public MuscleGroup MuscleGroups { get; private set; } = MuscleGroup.NONE;
        
        // Content references
        public Guid? ImageContentId { get; private set; }
        public Guid? VideoContentId { get; private set; }
        
        // Exercise metadata
        public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Unknown;
        public Equipment Equipment { get; private set; } = new Equipment();
        public string? Instructions { get; private set; }
        public bool IsActive { get; private set; } = true;
        
        // Audit
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        // Business methods
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ExerciseDomainException("Exercise name is required");
            
            if (name.Length > 100)
                throw new ExerciseDomainException("Exercise name cannot exceed 100 characters");
            
            Name = name.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetDescription(string? description)
        {
            if (!string.IsNullOrWhiteSpace(description) && description.Length > 1000)
                throw new ExerciseDomainException("Description cannot exceed 1000 characters");
            
            Description = description?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(ExerciseType type, DifficultyLevel difficulty, MuscleGroup muscleGroups)
        {
            Type = type;
            Difficulty = difficulty;
            MuscleGroups = muscleGroups;
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetEquipment(Equipment equipment)
        {
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetInstructions(string? instructions)
        {
            if (!string.IsNullOrWhiteSpace(instructions) && instructions.Length > 2000)
                throw new ExerciseDomainException("Instructions cannot exceed 2000 characters");
            
            Instructions = instructions?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void SetContentReferences(Guid? imageContentId, Guid? videoContentId)
        {
            ImageContentId = imageContentId;
            VideoContentId = videoContentId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsCardioExercise() => Type == ExerciseType.Cardio;
        public bool IsStrengthExercise() => Type == ExerciseType.Strength;
        public bool RequiresEquipment() => Equipment.Items.Any();
        public bool IsFullBodyExercise() => MuscleGroups.HasFlag(MuscleGroup.FULL_BODY);
        
    }
}
