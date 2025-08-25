using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Application.DTOs.Responses;

namespace FitnessApp.Modules.Users.Application.Mapping;

/// <summary>
/// Extension methods for mapping between domain entities and DTOs.
/// </summary>
public static class UserMappingExtensions
{
    public static UserDto MapToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Email,
            user.Username,
            user.Name.DisplayName,
            user.MapToProfileDto(),
            user.MapToSecurityDto(),
            user.Subscription?.MapToDto(),
            user.Preferences.Select(p => p.MapToDto()).ToList(),
            user.CreatedAt,
            user.UpdatedAt,
            user.LastLoginAt,
            user.IsActive
        );
    }

    public static UserListDto MapToListDto(this User user)
    {
        return new UserListDto(
            user.Id,
            user.Email,
            user.Username,
            user.Name.DisplayName,
            user.Gender,
            user.GetAge(),
            user.FitnessLevel,
            user.Role,
            user.EmailConfirmed,
            user.IsActive,
            user.CreatedAt,
            user.LastLoginAt
        );
    }

    public static UserProfileDto MapToProfileDto(this User user)
    {
        return new UserProfileDto(
            user.Name.FirstName,
            user.Name.LastName,
            user.Name.DisplayName,
            user.DateOfBirth?.Value,
            user.GetAge(),
            user.DateOfBirth?.GetAgeGroup(),
            user.Gender,
            user.Gender?.ToString(),
            user.PhysicalMeasurements.HeightCm,
            user.PhysicalMeasurements.WeightKg,
            user.PhysicalMeasurements.BMI,
            user.PhysicalMeasurements.GetBMICategory(),
            user.FitnessLevel,
            user.PrimaryFitnessGoal,
            user.HasCompletedProfile()
        );
    }

    public static UserSecurityDto MapToSecurityDto(this User user)
    {
        return new UserSecurityDto(
            user.Role,
            user.EmailConfirmed,
            user.TwoFactorEnabled,
            user.IsLockedOut(),
            user.LockoutEnd,
            user.AccessFailedCount
        );
    }

    public static UserSubscriptionDto MapToDto(this Subscription subscription)
    {
        var daysRemaining = subscription.IsActive 
            ? (int)(subscription.EndDate - DateTime.UtcNow).TotalDays 
            : 0;

        return new UserSubscriptionDto(
            subscription.Id,
            subscription.Level,
            subscription.StartDate,
            subscription.EndDate,
            subscription.IsActive,
            Math.Max(0, daysRemaining)
        );
    }

    public static UserPreferenceDto MapToDto(this Preference preference)
    {
        return new UserPreferenceDto(
            preference.Category,
            preference.Key,
            preference.Value,
            preference.CreatedAt,
            preference.UpdatedAt
        );
    }

    public static PagedUserResult MapToPagedResult(this IEnumerable<User> users, int totalCount, int page, int pageSize)
    {
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        
        return new PagedUserResult(
            users.Select(u => u.MapToListDto()),
            totalCount,
            page,
            pageSize,
            totalPages
        );
    }

    public static UserStatsDto MapToStatsDto(this IEnumerable<User> users)
    {
        var userList = users.ToList();
        
        return new UserStatsDto(
            userList.Count,
            userList.Count(u => u.IsActive),
            userList.Count(u => u.EmailConfirmed),
            userList.Count(u => u.CanAccessPremiumFeatures()),
            userList.GroupBy(u => u.Role.ToString())
                   .ToDictionary(g => g.Key, g => g.Count()),
            userList.Where(u => u.FitnessLevel.HasValue)
                   .GroupBy(u => u.FitnessLevel!.ToString())
                   .ToDictionary(g => g.Key ?? "Unknown", g => g.Count()),
            userList.Where(u => u.DateOfBirth != null)
                   .GroupBy(u => u.DateOfBirth!.GetAgeGroup())
                   .ToDictionary(g => g.Key ?? "Unknown", g => g.Count())
        );
    }
}
