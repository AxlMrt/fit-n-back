using FitnessApp.Modules.Users.Domain.Exceptions;
using FitnessApp.SharedKernel.Services;

namespace FitnessApp.Modules.Users.Domain.ValueObjects;

public sealed class PhysicalMeasurements : IEquatable<PhysicalMeasurements>
{
    public decimal? Height { get; }
    public decimal? Weight { get; }
    public string? HeightUnit { get; }
    public string? WeightUnit { get; }
    public decimal? BMI { get; }

    private PhysicalMeasurements(decimal? height, decimal? weight, string? heightUnit = null, string? weightUnit = null)
    {
        Height = height;
        Weight = weight;
        HeightUnit = heightUnit ?? (height.HasValue ? "cm" : null);
        WeightUnit = weightUnit ?? (weight.HasValue ? "kg" : null);
        BMI = CalculateBMI();
    }

    public static PhysicalMeasurements Create(decimal? height = null, decimal? weight = null, string? heightUnit = null, string? weightUnit = null)
    {
        if (height.HasValue)
        {
            if (height <= 0)
                throw new UserDomainException("Height must be greater than 0");
            
            // Validation adaptée selon l'unité
            var unit = heightUnit ?? "cm";
            var (minHeight, maxHeight) = unit.ToLower() switch
            {
                "ft" or "feet" => (1.6m, 8.2m), // ~50cm to ~250cm in feet
                "in" or "inches" => (20m, 98m), // ~50cm to ~250cm in inches
                _ => (50m, 250m) // cm par défaut
            };
            
            if (height < minHeight || height > maxHeight)
                throw new UserDomainException($"Height must be between {minHeight} and {maxHeight} {unit}");
        }

        if (weight.HasValue)
        {
            if (weight <= 0)
                throw new UserDomainException("Weight must be greater than 0");
            
            // Validation adaptée selon l'unité  
            var unit = weightUnit ?? "kg";
            var (minWeight, maxWeight) = unit.ToLower() switch
            {
                "lbs" or "pounds" => (44m, 661m), // ~20kg to ~300kg in pounds
                _ => (20m, 300m) // kg par défaut
            };
            
            if (weight < minWeight || weight > maxWeight)
                throw new UserDomainException($"Weight must be between {minWeight} and {maxWeight} {unit}");
        }

        return new PhysicalMeasurements(height, weight, heightUnit, weightUnit);
    }

    public static PhysicalMeasurements Empty => new PhysicalMeasurements(null, null, null, null);

    private decimal? CalculateBMI()
    {
        if (!Height.HasValue || !Weight.HasValue)
            return null;

        // Convertir en unités standard pour le calcul BMI (cm/kg)
        var heightCm = HeightUnit?.ToLower() switch
        {
            "ft" or "feet" => Height.Value * 30.48m,
            "in" or "inches" => Height.Value * 2.54m,
            _ => Height.Value // déjà en cm
        };
        
        var weightKg = WeightUnit?.ToLower() switch
        {
            "lbs" or "pounds" => Weight.Value * 0.453592m,
            _ => Weight.Value // déjà en kg
        };

        var heightM = heightCm / 100m;
        return Math.Round(weightKg / (heightM * heightM), 2);
    }

    public string GetBMICategory()
    {
        if (!BMI.HasValue) return "Unknown";

        return BMI.Value switch
        {
            < 18.5m => "Underweight",
            >= 18.5m and < 25m => "Normal weight",
            >= 25m and < 30m => "Overweight",
            >= 30m => "Obese",
        };
    }

    public PhysicalMeasurements UpdateHeight(decimal height, string? heightUnit = null)
    {
        return Create(height, Weight, heightUnit ?? HeightUnit, WeightUnit);
    }

    public PhysicalMeasurements UpdateWeight(decimal weight, string? weightUnit = null)
    {
        return Create(Height, weight, HeightUnit, weightUnit ?? WeightUnit);
    }

    /// <summary>
    /// Get height in the specified unit - converts from stored unit to requested unit
    /// </summary>
    public decimal? GetHeight(string unit = "cm")
    {
        if (!Height.HasValue) return null;
        
        // Si même unité que celle stockée, retourner directement
        if (HeightUnit?.Equals(unit, StringComparison.OrdinalIgnoreCase) == true)
            return Height.Value;
        
        // Convertir depuis l'unité stockée vers l'unité demandée
        var heightCm = HeightUnit?.ToLower() switch
        {
            "ft" or "feet" => Height.Value * 30.48m,
            "in" or "inches" => Height.Value * 2.54m,
            _ => Height.Value // déjà en cm
        };
        
        return unit.ToLower() switch
        {
            "ft" or "feet" => heightCm / 30.48m,
            "in" or "inches" => heightCm / 2.54m,
            _ => heightCm // cm demandé
        };
    }

    /// <summary>
    /// Get weight in the specified unit - converts from stored unit to requested unit
    /// </summary>
    public decimal? GetWeight(string unit = "kg")
    {
        if (!Weight.HasValue) return null;
        
        // Si même unité que celle stockée, retourner directement
        if (WeightUnit?.Equals(unit, StringComparison.OrdinalIgnoreCase) == true)
            return Weight.Value;
        
        // Convertir depuis l'unité stockée vers l'unité demandée
        var weightKg = WeightUnit?.ToLower() switch
        {
            "lbs" or "pounds" => Weight.Value * 0.453592m,
            _ => Weight.Value // déjà en kg
        };
        
        return unit.ToLower() switch
        {
            "lbs" or "pounds" => weightKg / 0.453592m,
            _ => weightKg // kg demandé
        };
    }

    public bool Equals(PhysicalMeasurements? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Height == other.Height && Weight == other.Weight;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is PhysicalMeasurements other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Height, Weight);

    public static bool operator ==(PhysicalMeasurements? left, PhysicalMeasurements? right) => Equals(left, right);

    public static bool operator !=(PhysicalMeasurements? left, PhysicalMeasurements? right) => !Equals(left, right);

    public override string ToString()
    {
        var parts = new List<string>();
        
        if (Height.HasValue)
            parts.Add($"Height: {Height}{HeightUnit}");
        
        if (Weight.HasValue)
            parts.Add($"Weight: {Weight}{WeightUnit}");
        
        if (BMI.HasValue)
            parts.Add($"BMI: {BMI} ({GetBMICategory()})");

        return parts.Count > 0 ? string.Join(", ", parts) : "No measurements";
    }
}
