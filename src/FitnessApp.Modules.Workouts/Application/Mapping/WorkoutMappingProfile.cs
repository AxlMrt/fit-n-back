using AutoMapper;
using FitnessApp.Modules.Workouts.Domain.Entities;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Workouts.Application.Mapping;

public class WorkoutMappingProfile : Profile
{
    public WorkoutMappingProfile()
    {
        CreateWorkoutMaps();
        CreateWorkoutPhaseMaps();
        CreateWorkoutExerciseMaps();
    }

    private void CreateWorkoutMaps()
    {
        // Request to Entity mappings
        CreateMap<CreateWorkoutDto, Workout>()
            .ConstructUsing((src, ctx) => new Workout(
                src.Name,
                src.Type,
                src.Category,
                src.Difficulty))
            .ForMember(dest => dest.Phases, opt => opt.Ignore())
            .AfterMap((src, dest, ctx) =>
            {
                if (!string.IsNullOrWhiteSpace(src.Description))
                {
                    dest.UpdateDetails(dest.Name, src.Description, dest.Difficulty);
                }
            });

        // Entity to Response mappings
        CreateMap<Workout, WorkoutDto>()
            .ForMember(dest => dest.PhaseCount, opt => opt.MapFrom(src => src.PhaseCount))
            .ForMember(dest => dest.TotalExercises, opt => opt.MapFrom(src => src.TotalExercises));

        CreateMap<Workout, WorkoutListDto>()
            .ForMember(dest => dest.PhaseCount, opt => opt.MapFrom(src => src.PhaseCount))
            .ForMember(dest => dest.TotalExercises, opt => opt.MapFrom(src => src.TotalExercises));
    }

    private void CreateWorkoutPhaseMaps()
    {
        // Request to Entity mappings
        CreateMap<CreateWorkoutPhaseDto, WorkoutPhase>()
            .ConstructUsing((src, ctx) => new WorkoutPhase(
                src.Type,
                src.Name,
                1))
            .ForMember(dest => dest.Exercises, opt => opt.Ignore());

        // Entity to Response mappings
        CreateMap<WorkoutPhase, WorkoutPhaseDto>();
    }

    private void CreateWorkoutExerciseMaps()
    {
        // Request to Entity mappings
        CreateMap<CreateWorkoutExerciseDto, WorkoutExercise>()
            .ConstructUsing((src, ctx) => new WorkoutExercise(
                src.ExerciseId,
                src.Sets,
                src.Reps,
                src.DurationSeconds,
                src.Distance,
                src.Weight,
                src.RestTimeSeconds,
                1));

        // Entity to Response mappings
        CreateMap<WorkoutExercise, WorkoutExerciseDto>();
    }
}
