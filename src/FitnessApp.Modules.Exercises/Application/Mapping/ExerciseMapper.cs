using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Application.Enums;
using FitnessApp.Modules.Exercises.Domain.Entities;

namespace FitnessApp.Modules.Exercises.Application.Mapping;

public class ExerciseMapper : IExerciseMapper
{
    public ExerciseResponse? MapToExercise(Exercise exercise)
    {
        if (exercise == null)
            return null;

        var response = new ExerciseResponse
        {
            Id = exercise.Id,
            Name = exercise.Name,
            Description = exercise.Description,
            Instructions = exercise.Instructions,
            CommonMistakes = exercise.CommonMistakes,
            Difficulty = Enum.Parse<DifficultyLevel>(exercise.DifficultyLevel),
            CaloriesBurnedPerMinute = exercise.EstimatedCaloriesBurn,
            CreatedAt = exercise.CreatedAt,
            UpdatedAt = exercise.UpdatedAt
        };

        // Map related collections if loaded
        if (exercise.ExerciseTags != null)
        {
            response.Tags = exercise.ExerciseTags
                .Select(et => MapToTag(et.Tag))
                .Where(tag => tag != null)
                .ToList()!;
        }

        if (exercise.TargetMuscleGroups != null)
        {
            response.MuscleGroups = exercise.TargetMuscleGroups
                .Select(mg => MapToExerciseMuscleGroup(mg))
                .Where(mg => mg != null)
                .ToList()!;
        }

        if (exercise.RequiredEquipment != null)
        {
            response.Equipment = exercise.RequiredEquipment
                .Select(re => MapToEquipment(re.Equipment))
                .Where(e => e != null)
                .ToList()!;
        }

        if (exercise.MediaAssetIds != null)
        {
            response.MediaAssetIds = exercise.MediaAssetIds.ToList();
        }

        return response;
    }

    public IEnumerable<ExerciseResponse> MapToExerciseList(IEnumerable<Exercise> exercises)
    {
        return exercises?.Select(MapToExercise).Where(e => e != null).Cast<ExerciseResponse>() ?? Enumerable.Empty<ExerciseResponse>();
    }

    public TagResponse? MapToTag(Tag tag)
    {
        if (tag == null)
            return null;

        return new TagResponse
        {
            Id = tag.Id,
            Name = tag.Name
        };
    }

    public EquipmentResponse? MapToEquipment(Equipment equipment)
    {
        if (equipment == null)
            return null;

        return new EquipmentResponse
        {
            Id = equipment.Id,
            Name = equipment.Name,
            Description = equipment.Description,
            //Category = equipment.Category,
            CreatedAt = equipment.CreatedAt,
            UpdatedAt = equipment.UpdatedAt
        };
    }

    public MuscleGroupResponse? MapToMuscleGroup(MuscleGroup muscleGroup)
    {
        if (muscleGroup == null)
            return null;

        return new MuscleGroupResponse
        {
            Id = muscleGroup.Id,
            Name = muscleGroup.Name,
            Description = muscleGroup.Description,
        };
    }

    public IEnumerable<MuscleGroupResponse> MapToMuscleGroupList(IEnumerable<MuscleGroup> muscleGroups)
    {
        return muscleGroups?.Select(MapToMuscleGroup).Where(mg => mg != null).Cast<MuscleGroupResponse>() ?? Enumerable.Empty<MuscleGroupResponse>();
    }

    public ExerciseMuscleGroupReponse? MapToExerciseMuscleGroup(ExerciseMuscleGroup exerciseMuscleGroup)
    {
        if (exerciseMuscleGroup == null || exerciseMuscleGroup.MuscleGroup == null)
            return null;

        return new ExerciseMuscleGroupReponse
        {
            MuscleGroup = new MuscleGroupResponse
            {
                Id = exerciseMuscleGroup.MuscleGroup.Id,
                Name = exerciseMuscleGroup.MuscleGroup.Name,
                Description = exerciseMuscleGroup.MuscleGroup.Description
            },
            IsPrimary = exerciseMuscleGroup.IsPrimary
        };
    }

    public MediaResourceResponse? MapToMediaResource(MediaResource mediaResource)
    {
        if (mediaResource == null)
            return null;

        return new MediaResourceResponse
        {
            Id = mediaResource.Id,
            Url = mediaResource.Url,
            Type = Enum.Parse<MediaType>(mediaResource.Type),
            Description = mediaResource.Description
        };
    }
}