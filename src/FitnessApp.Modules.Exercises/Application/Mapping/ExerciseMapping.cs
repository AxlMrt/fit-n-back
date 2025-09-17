using AutoMapper;
using FitnessApp.Modules.Exercises.Application.Mapping.Converters;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.SharedKernel.DTOs.Requests;
using FitnessApp.SharedKernel.DTOs.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Exercises.Application.Mapping;

public class ExerciseMappingProfile : Profile
{
    public ExerciseMappingProfile()
    {
        CreateMap<List<string>, MuscleGroup>().ConvertUsing(new MuscleGroupConverter());
        CreateMap<MuscleGroup, List<string>>().ConvertUsing(new MuscleGroupListConverter());
        
        CreateMap<List<string>, Equipment>().ConvertUsing(new EquipmentConverter());
        CreateMap<Equipment, List<string>>().ConvertUsing(new EquipmentListConverter());

        CreateMap<Exercise, ExerciseDto>()
            .ForMember(dest => dest.MuscleGroups, opt => opt.MapFrom(src => src.MuscleGroups))
            .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => src.Equipment));

        CreateMap<Exercise, ExerciseListDto>()
            .ForMember(dest => dest.MuscleGroups, opt => opt.MapFrom(src => src.MuscleGroups))
            .ForMember(dest => dest.RequiresEquipment, opt => opt.MapFrom(src => src.Equipment != Equipment.None));

        CreateMap<CreateExerciseDto, Exercise>()
            .ConstructUsing((src, ctx) => new Exercise(
                src.Name,
                src.Type,
                src.Difficulty,
                ctx.Mapper.Map<MuscleGroup>(src.MuscleGroups),
                ctx.Mapper.Map<Equipment>(src.Equipment)
            ))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .AfterMap((src, dest, ctx) =>
            {
                if (!string.IsNullOrWhiteSpace(src.Description))
                    dest.SetDescription(src.Description);
                if (!string.IsNullOrWhiteSpace(src.Instructions))
                    dest.SetInstructions(src.Instructions);
                if (src.ImageContentId.HasValue || src.VideoContentId.HasValue)
                    dest.SetContentReferences(src.ImageContentId, src.VideoContentId);
            });
    }
}
