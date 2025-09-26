using AutoMapper;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Mappings.Converters;

/// <summary>
/// Converter from string to Gender enum with case-insensitive matching
/// </summary>
public class GenderConverter : ITypeConverter<string, Gender>
{
    public Gender Convert(string source, Gender destination, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source))
            return Gender.PreferNotToSay;

        return Enum.TryParse<Gender>(source, true, out var result) ? result : Gender.PreferNotToSay;
    }
}

/// <summary>
/// Converter from string to FitnessLevel enum with case-insensitive matching
/// </summary>
public class FitnessLevelConverter : ITypeConverter<string, FitnessLevel>
{
    public FitnessLevel Convert(string source, FitnessLevel destination, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source))
            return FitnessLevel.Beginner;

        return Enum.TryParse<FitnessLevel>(source, true, out var result) ? result : FitnessLevel.Beginner;
    }
}

/// <summary>
/// Converter from string to FitnessGoal enum with case-insensitive matching
/// </summary>
public class FitnessGoalConverter : ITypeConverter<string, FitnessGoal>
{
    public FitnessGoal Convert(string source, FitnessGoal destination, ResolutionContext context)
    {
        if (string.IsNullOrWhiteSpace(source))
            return FitnessGoal.Wellness;

        return Enum.TryParse<FitnessGoal>(source, true, out var result) ? result : FitnessGoal.Wellness;
    }
}
