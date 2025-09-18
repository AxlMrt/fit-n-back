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
                src.Difficulty,
                src.EstimatedDurationMinutes))
            .ForMember(dest => dest.Phases, opt => opt.Ignore()) // Ignore read-only collection
            .AfterMap((src, dest, ctx) =>
            {
                if (!string.IsNullOrWhiteSpace(src.Description))
                {
                    dest.UpdateDetails(dest.Name, src.Description);
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
                src.EstimatedDurationMinutes,
                1)) // Order will be set properly later
            .ForMember(dest => dest.Exercises, opt => opt.Ignore()); // Ignore read-only collection

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
                1)); // Order will be set properly later

        // Entity to Response mappings
        CreateMap<WorkoutExercise, WorkoutExerciseDto>();
    }
}
