using FitnessApp.Modules.Exercises.Domain.Enums;

namespace FitnessApp.Modules.Exercises.Application.DTOs
{
    public class ExerciseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ExerciseType Type { get; set; }

        // Expose muscle groups as string list for clients
        public List<string> MuscleGroups { get; set; } = new();

        // Content module resource ids
        public Guid? ImageContentId { get; set; }
        public Guid? VideoContentId { get; set; }

        // Use human-friendly fields for clients; mapping will convert
        public DifficultyLevelDto Difficulty { get; set; } = DifficultyLevelDto.Unknown;
        public List<string> Equipment { get; set; } = new();
    }

    public class CreateExerciseDto
    {
        public string Name { get; set; } = string.Empty;
        public ExerciseType Type { get; set; }
        public List<string> MuscleGroups { get; set; } = new();
        public DifficultyLevelDto Difficulty { get; set; } = DifficultyLevelDto.Unknown;
        public List<string> Equipment { get; set; } = new();
    }

    public enum DifficultyLevelDto
    {
        Unknown = 0,
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4,
        Other = 100
    }
}
