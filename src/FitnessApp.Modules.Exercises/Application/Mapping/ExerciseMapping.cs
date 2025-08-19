using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;

namespace FitnessApp.Modules.Exercises.Application.Mapping
{
    public static class ExerciseMapping
    {
        public static ExerciseDto ToDto(this Exercise e)
        {
            return new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Type = e.Type,
                MuscleGroups = e.MuscleGroups == MuscleGroup.NONE ? new List<string>() :
                    Enum.GetValues(typeof(MuscleGroup)).Cast<MuscleGroup>()
                        .Where(m => m != MuscleGroup.NONE && e.MuscleGroups.HasFlag(m))
                        .Select(m => m.ToString())
                        .ToList(),
                ImageContentId = e.ImageContentId,
                VideoContentId = e.VideoContentId,
                // convert domain Difficulty enum to DTO enum
                Difficulty = (DifficultyLevelDto)e.Difficulty,
                // convert Equipment value object to list of strings
                Equipment = e.Equipment?.Items ?? new List<string>(),
            };
        }

        public static Exercise ToEntity(this ExerciseDto d)
        {
            var muscle = MuscleGroup.NONE;
            foreach (var mg in d.MuscleGroups ?? new List<string>())
            {
                if (Enum.TryParse<MuscleGroup>(mg, true, out var parsed))
                    muscle |= parsed;
            }

            return new Exercise
            {
                Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
                Name = d.Name,
                Type = d.Type,
                MuscleGroups = muscle,
                ImageContentId = d.ImageContentId,
                VideoContentId = d.VideoContentId,
                Difficulty = (DifficultyLevel)d.Difficulty,
                Equipment = new Equipment(d.Equipment),
            };
        }
    }
}
