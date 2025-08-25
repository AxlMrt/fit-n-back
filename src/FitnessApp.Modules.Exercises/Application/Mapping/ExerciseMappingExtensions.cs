using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;

namespace FitnessApp.Modules.Exercises.Application.Mapping
{
    public static class ExerciseMappingExtensions
    {
        public static ExerciseDto MapToDto(this Exercise exercise)
        {
            return new ExerciseDto(
                exercise.Id,
                exercise.Name,
                exercise.Description,
                exercise.Type,
                ConvertMuscleGroupsToList(exercise.MuscleGroups),
                exercise.ImageContentId,
                exercise.VideoContentId,
                exercise.Difficulty,
                exercise.Equipment.Items.ToList(),
                exercise.Instructions,
                exercise.IsActive,
                exercise.CreatedAt,
                exercise.UpdatedAt
            );
        }

        public static ExerciseListDto MapToListDto(this Exercise exercise)
        {
            return new ExerciseListDto(
                exercise.Id,
                exercise.Name,
                exercise.Type,
                exercise.Difficulty,
                ConvertMuscleGroupsToList(exercise.MuscleGroups),
                exercise.Equipment.Items.Any(),
                exercise.IsActive
            );
        }

        private static List<string> ConvertMuscleGroupsToList(MuscleGroup muscleGroups)
        {
            if (muscleGroups == MuscleGroup.NONE)
                return new List<string>();

            return Enum.GetValues<MuscleGroup>()
                .Where(mg => mg != MuscleGroup.NONE && muscleGroups.HasFlag(mg))
                .Select(mg => mg.ToString())
                .ToList();
        }
    }
}
