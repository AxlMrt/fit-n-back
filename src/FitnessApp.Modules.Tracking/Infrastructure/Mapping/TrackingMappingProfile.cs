using AutoMapper;
using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.SharedKernel.DTOs.Responses;

namespace FitnessApp.Modules.Tracking.Infrastructure.Mapping;

/// <summary>
/// AutoMapper profile for Tracking module
/// </summary>
public class TrackingMappingProfile : Profile
{
    public TrackingMappingProfile()
    {
        CreateMap<WorkoutSession, WorkoutSessionDto>()
            .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.Exercises));

        CreateMap<WorkoutSession, WorkoutSessionListDto>()
            .ForMember(dest => dest.ExerciseCount, opt => opt.MapFrom(src => src.ExerciseCount));

        CreateMap<WorkoutSessionExercise, WorkoutSessionExerciseDto>()
            .ForMember(dest => dest.Sets, opt => opt.MapFrom(src => src.Sets))
            .ForMember(dest => dest.PerformanceDisplay, opt => opt.MapFrom(src => src.GetPerformanceDisplay()));

        CreateMap<WorkoutSessionSet, WorkoutSessionSetDto>()
            .ForMember(dest => dest.DisplayText, opt => opt.MapFrom(src => src.GetDisplayText()));

        CreateMap<UserMetric, UserMetricDto>()
            .ForMember(dest => dest.DisplayValue, opt => opt.MapFrom(src => src.GetDisplayValue()));

        CreateMap<PlannedWorkout, PlannedWorkoutDto>()
            .ForMember(dest => dest.IsOverdue, opt => opt.MapFrom(src => src.IsOverdue()))
            .ForMember(dest => dest.DaysUntilScheduled, opt => opt.MapFrom(src => src.DaysUntilScheduled()));
    }
}
