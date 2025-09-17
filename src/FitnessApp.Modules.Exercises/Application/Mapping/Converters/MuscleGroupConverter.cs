using AutoMapper;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Exercises.Application.Mapping.Converters;

public class MuscleGroupConverter : ITypeConverter<List<string>, MuscleGroup>
{
    public MuscleGroup Convert(List<string> source, MuscleGroup destination, ResolutionContext context)
    {
        if (source == null || !source.Any())
            return MuscleGroup.None;

        var result = MuscleGroup.None;
        foreach (var mg in source)
        {
            // Gérer le cas spécial "Full Body" -> "Full_Body"
            var normalizedName = mg?.Trim().Replace(" ", "_") ?? "";
            
            if (Enum.TryParse<MuscleGroup>(normalizedName, true, out var parsed))
                result |= parsed;
        }

        return result;
    }
}
            

public class MuscleGroupListConverter : ITypeConverter<MuscleGroup, List<string>>
{
    public List<string> Convert(MuscleGroup source, List<string> destination, ResolutionContext context)
    {
        if (source == MuscleGroup.None)
            return new List<string>();

        return Enum.GetValues<MuscleGroup>()
            .Where(mg => mg != MuscleGroup.None && source.HasFlag(mg))
            .Select(mg => mg == MuscleGroup.Full_Body ? "Full Body" : mg.ToString())
            .ToList();
    }
}