namespace FitnessApp.Modules.Exercises.Application.DTOs.Responses;

public class ExerciseMuscleGroupReponse
{
    public MuscleGroupResponse MuscleGroup { get; set; } = null!;
    public bool IsPrimary { get; set; }
}