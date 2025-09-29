using FitnessApp.SharedKernel.Exceptions;

namespace FitnessApp.Modules.Users.Domain.Exceptions;

/// <summary>
/// Exception levée lorsqu'une règle métier est violée dans le domaine User.
/// </summary>
public sealed class UserDomainException : DomainException
{
    public UserDomainException(string errorCode, string message) 
        : base("Users", errorCode, message)
    {
    }

    public UserDomainException(string errorCode, string message, Exception innerException) 
        : base("Users", errorCode, message, innerException)
    {
    }

    // Factory methods for common user domain errors
    public static UserDomainException InvalidAge(int age) =>
        new("INVALID_AGE", $"Age {age} is not valid. Users must be between 13 and 120 years old");

    public static UserDomainException InvalidPhysicalMeasurement(string measurement, decimal value, string unit) =>
        new("INVALID_PHYSICAL_MEASUREMENT", $"{measurement} value {value} {unit} is not valid");

    public static UserDomainException FutureBirthDate() =>
        new("FUTURE_BIRTH_DATE", "Date of birth cannot be in the future");
    
    // Physical measurements factory methods
    public static UserDomainException HeightMustBeGreaterThanZero() =>
        new("HEIGHT_MUST_BE_POSITIVE", "Height must be greater than 0");
    
    public static UserDomainException HeightOutOfRange(decimal minHeight, decimal maxHeight, string unit) =>
        new("HEIGHT_OUT_OF_RANGE", $"Height must be between {minHeight} and {maxHeight} {unit}");
    
    public static UserDomainException WeightMustBeGreaterThanZero() =>
        new("WEIGHT_MUST_BE_POSITIVE", "Weight must be greater than 0");
    
    public static UserDomainException WeightOutOfRange(decimal minWeight, decimal maxWeight, string unit) =>
        new("WEIGHT_OUT_OF_RANGE", $"Weight must be between {minWeight} and {maxWeight} {unit}");
    
    // Full name factory methods
    public static UserDomainException NameTooLong(string parameterName) =>
        new("NAME_TOO_LONG", $"{parameterName} cannot exceed 50 characters");
    
    public static UserDomainException NameContainsInvalidCharacters(string parameterName) =>
        new("NAME_INVALID_CHARACTERS", $"{parameterName} contains invalid characters");
    
    // Date of birth factory methods
    public static UserDomainException UnrealisticAge() =>
        new("UNREALISTIC_AGE", "Date of birth indicates an unrealistic age");
    
    public static UserDomainException UserTooYoung() =>
        new("USER_TOO_YOUNG", "Users must be at least 13 years old");
    
    // Preference factory methods
    public static UserDomainException UserIdRequired() =>
        new("USER_ID_REQUIRED", "User ID is required");
    
    public static UserDomainException PreferenceKeyRequired() =>
        new("PREFERENCE_KEY_REQUIRED", "Preference key is required");
    
    public static UserDomainException PreferenceKeyTooLong() =>
        new("PREFERENCE_KEY_TOO_LONG", "Preference key cannot exceed 100 characters");
    
    public static UserDomainException PreferenceValueTooLong() =>
        new("PREFERENCE_VALUE_TOO_LONG", "Preference value cannot exceed 1000 characters");
    
    public static UserDomainException PreferenceConversionFailed(string value, string targetType) =>
        new("PREFERENCE_CONVERSION_FAILED", $"Cannot convert preference value '{value}' to type {targetType}");
    
    // User not found factory methods
    public static UserDomainException UserNotFound(Guid userId) =>
        new("USER_NOT_FOUND", $"User with ID {userId} was not found");
    
    public static UserDomainException UserNotFound(string identifier) =>
        new("USER_NOT_FOUND", $"User {identifier} was not found");
    
    // User profile operations factory methods
    public static UserDomainException UserProfileAlreadyExists() =>
        new("USER_PROFILE_ALREADY_EXISTS", "User profile already exists");
    
    public static UserDomainException UserProfileNotFound(Guid userId) =>
        new("USER_PROFILE_NOT_FOUND", $"User profile not found for user {userId}");
    
    public static UserDomainException FailedToDeleteUserProfile(Guid userId) =>
        new("FAILED_TO_DELETE_USER_PROFILE", $"Failed to delete user profile for user {userId}");
    
    // Subscription factory methods
    public static UserDomainException NoActiveSubscriptionFound() =>
        new("NO_ACTIVE_SUBSCRIPTION", "No active subscription found");
    
    public static UserDomainException NoSubscriptionToRenew() =>
        new("NO_SUBSCRIPTION_TO_RENEW", "No subscription found to renew");
}
