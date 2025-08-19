using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;

namespace FitnessApp.Modules.Exercises.Domain.Entities
{
    public class Exercise
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ExerciseType Type { get; set; }

        // Flags enum stored as integer in DB
        public MuscleGroup MuscleGroups { get; set; } = MuscleGroup.NONE;

        // References to Content module resources - store as nullable GUIDs
        public Guid? ImageContentId { get; set; }
        public Guid? VideoContentId { get; set; }

        // Extensible metadata: strongly typed
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Unknown;
        public Equipment Equipment { get; set; } = new Equipment();

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties could be added later if modules are in same solution
    }
}
