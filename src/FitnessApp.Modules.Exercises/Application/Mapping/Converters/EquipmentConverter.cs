using AutoMapper;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Exercises.Application.Mapping.Converters;

public class EquipmentConverter : ITypeConverter<List<string>, Equipment>
{
    public Equipment Convert(List<string> source, Equipment destination, ResolutionContext context)
    {
        if (source == null || !source.Any())
            return Equipment.None;

        Equipment result = Equipment.None;
        
        foreach (var item in source)
        {
            // Normaliser les noms d'équipement
            var normalizedName = NormalizeEquipmentName(item);
            
            if (Enum.TryParse<Equipment>(normalizedName, true, out var equipmentItem))
            {
                result |= equipmentItem;
            }
        }

        return result;
    }

    private static string NormalizeEquipmentName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        
        var normalized = name.Trim();
        
        // Gérer les variations communes
        return normalized.ToLowerInvariant() switch
        {
            "barbell" => "Barbells",
            "dumbbell" => "Dumbbells", 
            "kettlebell" => "Kettlebells",
            "pull up bar" or "pullupbar" => "PullUpBar",
            "dip bars" or "dipbars" => "DipBars",
            "incline bench" or "inclinebench" => "InclineBench",
            "resistance bands" or "resistancebands" => "ResistanceBands",
            _ => normalized
        };
    }
}

public class EquipmentListConverter : ITypeConverter<Equipment, List<string>>
{
    public List<string> Convert(Equipment source, List<string> destination, ResolutionContext context)
    {
        if (source == Equipment.None)
            return new List<string>();

        var result = new List<string>();
        
        foreach (Equipment equipment in Enum.GetValues<Equipment>())
        {
            if (equipment != Equipment.None && source.HasFlag(equipment))
            {
                result.Add(equipment.ToString());
            }
        }

        return result;
    }
}
