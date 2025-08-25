using AutoMapper;
using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Domain.Entities;
using FitnessApp.Modules.Exercises.Domain.Enums;
using FitnessApp.Modules.Exercises.Domain.ValueObjects;

namespace FitnessApp.Modules.Exercises.Application.Mapping
{
    public class ExerciseMappingProfile : Profile
    {
        public ExerciseMappingProfile()
        {
            CreateMap<Exercise, ExerciseDto>()
                .ForMember(dest => dest.MuscleGroups, opt => opt.MapFrom(src => ConvertMuscleGroupsToList(src.MuscleGroups)))
                .ForMember(dest => dest.Equipment, opt => opt.MapFrom(src => src.Equipment.Items.ToList()));

            CreateMap<Exercise, ExerciseListDto>()
                .ForMember(dest => dest.MuscleGroups, opt => opt.MapFrom(src => ConvertMuscleGroupsToList(src.MuscleGroups)))
                .ForMember(dest => dest.RequiresEquipment, opt => opt.MapFrom(src => src.Equipment.Items.Any()));

            CreateMap<CreateExerciseDto, Exercise>()
                .ConstructUsing((src, ctx) => new Exercise(
                    src.Name,
                    src.Type,
                    src.Difficulty,
                    ConvertMuscleGroupsList(src.MuscleGroups),
                    new Equipment(src.Equipment)
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

        private static List<string> ConvertMuscleGroupsToList(MuscleGroup muscleGroups)
        {
            if (muscleGroups == MuscleGroup.NONE)
                return new List<string>();

            return Enum.GetValues<MuscleGroup>()
                .Where(mg => mg != MuscleGroup.NONE && muscleGroups.HasFlag(mg))
                .Select(mg => mg.ToString())
                .ToList();
        }

        private static MuscleGroup ConvertMuscleGroupsList(List<string> muscleGroups)
        {
            if (muscleGroups == null || !muscleGroups.Any())
                return MuscleGroup.NONE;

            var result = MuscleGroup.NONE;
            foreach (var mg in muscleGroups)
            {
                if (Enum.TryParse<MuscleGroup>(mg, true, out var parsed))
                    result |= parsed;
            }

            return result;
        }
    }
}
