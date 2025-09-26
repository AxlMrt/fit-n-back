using AutoMapper;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Application.Mappings.Converters;
using FitnessApp.SharedKernel.DTOs.Users.Responses;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Mappings;

public class UsersMappingProfile : Profile
{
    public UsersMappingProfile()
    {
        // Add enum converters
        CreateMap<string, Gender>().ConvertUsing<GenderConverter>();
        CreateMap<string, FitnessLevel>().ConvertUsing<FitnessLevelConverter>();
        CreateMap<string, FitnessGoal>().ConvertUsing<FitnessGoalConverter>();

        CreateMap<UserProfile, UserProfileResponse>()
            .ConstructUsing((src, context) =>
            {
                // Try to get preferred units from context, fallback to defaults
                var heightUnit = context.Items.ContainsKey("HeightUnit") ? (string)context.Items["HeightUnit"] : "cm";
                var weightUnit = context.Items.ContainsKey("WeightUnit") ? (string)context.Items["WeightUnit"] : "kg";
                
                return new UserProfileResponse(
                    src.UserId,
                    src.Name.FirstName ?? string.Empty,
                    src.Name.LastName ?? string.Empty,
                    src.DateOfBirth != null ? src.DateOfBirth.Value : (DateTime?)null,
                    src.GetAge(),
                    src.Gender,
                    src.PhysicalMeasurements.GetHeight(heightUnit),
                    src.PhysicalMeasurements.GetWeight(weightUnit),
                    heightUnit,
                    weightUnit,
                    src.GetBMI(),
                    src.FitnessLevel,
                    src.FitnessGoal,
                    src.Subscription != null ? new SubscriptionResponse(
                        src.Subscription.Id,
                        src.Subscription.Level,
                        src.Subscription.StartDate,
                        src.Subscription.EndDate,
                        src.Subscription.IsActive
                    ) : null,
                    src.HasCompletedProfile(),
                    src.CanAccessPremiumFeatures(),
                    src.CreatedAt,
                    src.UpdatedAt
                );
            });

        CreateMap<UserProfile, UserProfileSummaryResponse>()
            .ConstructUsing(src => new UserProfileSummaryResponse(
                src.UserId,
                GetFullName(src.Name.FirstName, src.Name.LastName),
                src.GetAge(),
                src.Gender,
                src.FitnessLevel,
                src.Subscription != null ? src.Subscription.Level : null,
                src.HasCompletedProfile()
            ));

        CreateMap<Subscription, SubscriptionResponse>()
            .ConstructUsing(src => new SubscriptionResponse(
                src.Id,
                src.Level,
                src.StartDate,
                src.EndDate,
                src.IsActive
            ));

        CreateMap<Preference, PreferenceResponse>()
            .ConstructUsing(src => new PreferenceResponse(
                src.Id,
                src.Category,
                src.Key,
                src.Value ?? string.Empty,
                src.CreatedAt,
                src.UpdatedAt
            ));
    }

    private static string GetFullName(string? firstName, string? lastName)
    {
        var fullName = $"{firstName} {lastName}".Trim();
        return string.IsNullOrEmpty(fullName) ? "User" : fullName;
    }
}
