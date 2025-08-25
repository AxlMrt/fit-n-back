using FitnessApp.Modules.Users.Domain.Exceptions;

namespace FitnessApp.Modules.Users.Domain.ValueObjects;

/// <summary>
/// Value Object representing physical measurements with validation.
/// </summary>
public sealed class PhysicalMeasurements : IEquatable<PhysicalMeasurements>
{
    public decimal? HeightCm { get; }
    public decimal? WeightKg { get; }
    public decimal? BMI { get; }

    private PhysicalMeasurements(decimal? heightCm, decimal? weightKg)
    {
        HeightCm = heightCm;
        WeightKg = weightKg;
        BMI = CalculateBMI();
    }

    public static PhysicalMeasurements Create(decimal? heightCm = null, decimal? weightKg = null)
    {
        if (heightCm.HasValue)
        {
            if (heightCm <= 0)
                throw new UserDomainException("Height must be greater than 0");
            
            if (heightCm < 50 || heightCm > 250)
                throw new UserDomainException("Height must be between 50cm and 250cm");
        }

        if (weightKg.HasValue)
        {
            if (weightKg <= 0)
                throw new UserDomainException("Weight must be greater than 0");
            
            if (weightKg < 20 || weightKg > 300)
                throw new UserDomainException("Weight must be between 20kg and 300kg");
        }

        return new PhysicalMeasurements(heightCm, weightKg);
    }

    public static PhysicalMeasurements Empty => new PhysicalMeasurements(null, null);

    private decimal? CalculateBMI()
    {
        if (!HeightCm.HasValue || !WeightKg.HasValue)
            return null;

        var heightM = HeightCm.Value / 100m;
        return Math.Round(WeightKg.Value / (heightM * heightM), 2);
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

    public PhysicalMeasurements UpdateHeight(decimal heightCm)
    {
        return Create(heightCm, WeightKg);
    }

    public PhysicalMeasurements UpdateWeight(decimal weightKg)
    {
        return Create(HeightCm, weightKg);
    }

    public bool Equals(PhysicalMeasurements? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return HeightCm == other.HeightCm && WeightKg == other.WeightKg;
    }

    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is PhysicalMeasurements other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(HeightCm, WeightKg);

    public static bool operator ==(PhysicalMeasurements? left, PhysicalMeasurements? right) => Equals(left, right);

    public static bool operator !=(PhysicalMeasurements? left, PhysicalMeasurements? right) => !Equals(left, right);

    public override string ToString()
    {
        var parts = new List<string>();
        
        if (HeightCm.HasValue)
            parts.Add($"Height: {HeightCm}cm");
        
        if (WeightKg.HasValue)
            parts.Add($"Weight: {WeightKg}kg");
        
        if (BMI.HasValue)
            parts.Add($"BMI: {BMI} ({GetBMICategory()})");

        return parts.Count > 0 ? string.Join(", ", parts) : "No measurements";
    }
}
