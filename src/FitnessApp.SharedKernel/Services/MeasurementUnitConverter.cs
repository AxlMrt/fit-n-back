namespace FitnessApp.SharedKernel.Services;

/// <summary>
/// Service for converting between different measurement units
/// </summary>
public static class MeasurementUnitConverter
{
    #region Height Conversions

    /// <summary>
    /// Convert height to centimeters (standard storage unit)
    /// </summary>
    public static decimal ConvertHeightToCentimeters(decimal value, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "cm" => value,
            "ft" => value * 30.48m, // 1 foot = 30.48 cm
            "in" => value * 2.54m,   // 1 inch = 2.54 cm
            _ => throw new ArgumentException($"Unsupported height unit: {unit}")
        };
    }

    /// <summary>
    /// Convert height from centimeters to specified unit
    /// </summary>
    public static decimal ConvertHeightFromCentimeters(decimal centimeters, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "cm" => centimeters,
            "ft" => Math.Round(centimeters / 30.48m, 2),
            "in" => Math.Round(centimeters / 2.54m, 1),
            _ => throw new ArgumentException($"Unsupported height unit: {unit}")
        };
    }

    /// <summary>
    /// Parse height from string format like "5'10\"" or "180cm"
    /// </summary>
    public static (decimal value, string unit) ParseHeight(string input)
    {
        input = input.Trim();
        
        // Handle feet and inches format: "5'10"" or "5ft 10in"
        if (input.Contains('\'') || input.Contains("ft"))
        {
            var parts = input.Replace("\"", "").Replace("ft", "").Replace("in", "").Replace("'", " ").Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 && decimal.TryParse(parts[0], out var feet) && decimal.TryParse(parts[1], out var inches))
            {
                var totalInches = (feet * 12) + inches;
                return (totalInches, "in");
            }
        }
        
        // Handle simple numeric with unit
        if (input.EndsWith("cm", StringComparison.OrdinalIgnoreCase))
            return (decimal.Parse(input[..^2]), "cm");
        if (input.EndsWith("in", StringComparison.OrdinalIgnoreCase))
            return (decimal.Parse(input[..^2]), "in");
        
        // Default to cm if no unit specified
        return (decimal.Parse(input), "cm");
    }

    #endregion

    #region Weight Conversions

    /// <summary>
    /// Convert weight to kilograms (standard storage unit)
    /// </summary>
    public static decimal ConvertWeightToKilograms(decimal value, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "kg" => value,
            "lbs" or "lb" => value * 0.453592m, // 1 pound = 0.453592 kg
            _ => throw new ArgumentException($"Unsupported weight unit: {unit}")
        };
    }

    /// <summary>
    /// Convert weight from kilograms to specified unit
    /// </summary>
    public static decimal ConvertWeightFromKilograms(decimal kilograms, string unit)
    {
        return unit.ToLowerInvariant() switch
        {
            "kg" => kilograms,
            "lbs" or "lb" => Math.Round(kilograms / 0.453592m, 1),
            _ => throw new ArgumentException($"Unsupported weight unit: {unit}")
        };
    }

    #endregion

    #region User Preference Units

    /// <summary>
    /// Get default units based on user's locale or preference
    /// </summary>
    public static (string heightUnit, string weightUnit) GetDefaultUnits(string? locale = null)
    {
        // US uses imperial, most others use metric
        if (locale?.StartsWith("en-US", StringComparison.OrdinalIgnoreCase) == true)
        {
            return ("ft", "lbs");
        }
        
        return ("cm", "kg");
    }

    /// <summary>
    /// Validate that units are supported
    /// </summary>
    public static bool IsValidHeightUnit(string unit)
    {
        return unit.ToLowerInvariant() is "cm" or "ft" or "in";
    }

    public static bool IsValidWeightUnit(string unit)
    {
        return unit.ToLowerInvariant() is "kg" or "lbs" or "lb";
    }

    #endregion
}
