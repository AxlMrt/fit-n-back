using FitnessApp.Modules.Exercises.Application.DTOs.Responses;
using FitnessApp.Modules.Exercises.Domain.Entities;

namespace FitnessApp.Modules.Exercises.Application.Mapping;

public interface IExerciseMapper
{
    ExerciseResponse? MapToExercise(Exercise exercise);
    IEnumerable<ExerciseResponse> MapToExerciseList(IEnumerable<Exercise> exercises);
    TagResponse? MapToTag(Tag tag);
    EquipmentResponse? MapToEquipment(Equipment equipment);
    MuscleGroupResponse? MapToMuscleGroup(MuscleGroup muscleGroup);
    IEnumerable<MuscleGroupResponse> MapToMuscleGroupList(IEnumerable<MuscleGroup> muscleGroups);
    ExerciseMuscleGroupReponse? MapToExerciseMuscleGroup(ExerciseMuscleGroup exerciseMuscleGroup);
    MediaResourceResponse? MapToMediaResource(MediaResource mediaResource);
}