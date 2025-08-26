using FluentValidation;
using FitnessApp.SharedKernel.DTOs.UserProfile.Requests;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Validators;

/// <summary>
/// Validator for CreateUserProfileRequest.
/// </summary>
public class CreateUserProfileRequestValidator : AbstractValidator<CreateUserProfileRequest>
{
    public CreateUserProfileRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("FirstName must be between 1 and 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(1, 100)
            .WithMessage("LastName must be between 1 and 100 characters");

        RuleFor(x => x.DateOfBirth)
            .Must(BeValidAge)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Age must be between 13 and 120 years");

        RuleFor(x => x.HeightInMeters)
            .InclusiveBetween(0.1m, 3.0m)
            .When(x => x.HeightInMeters.HasValue)
            .WithMessage("Height must be between 0.1m and 3.0m");

        RuleFor(x => x.WeightInKilograms)
            .InclusiveBetween(1m, 1000m)
            .When(x => x.WeightInKilograms.HasValue)
            .WithMessage("Weight must be between 1kg and 1000kg");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Bio))
            .WithMessage("Bio cannot exceed 500 characters");
    }

    private bool BeValidAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;
        
        var age = DateTime.Today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age))
            age--;
            
        return age >= 13 && age <= 120;
    }
}

/// <summary>
/// Validator for UpdateUserProfileRequest.
/// </summary>
public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Length(1, 100)
            .When(x => !string.IsNullOrEmpty(x.FirstName))
            .WithMessage("FirstName must be between 1 and 100 characters");

        RuleFor(x => x.LastName)
            .Length(1, 100)
            .When(x => !string.IsNullOrEmpty(x.LastName))
            .WithMessage("LastName must be between 1 and 100 characters");

        RuleFor(x => x.DateOfBirth)
            .Must(BeValidAge)
            .When(x => x.DateOfBirth.HasValue)
            .WithMessage("Age must be between 13 and 120 years");

        RuleFor(x => x.HeightInMeters)
            .InclusiveBetween(0.1m, 3.0m)
            .When(x => x.HeightInMeters.HasValue)
            .WithMessage("Height must be between 0.1m and 3.0m");

        RuleFor(x => x.WeightInKilograms)
            .InclusiveBetween(1m, 1000m)
            .When(x => x.WeightInKilograms.HasValue)
            .WithMessage("Weight must be between 1kg and 1000kg");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio != null)
            .WithMessage("Bio cannot exceed 500 characters");
    }

    private bool BeValidAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;
        
        var age = DateTime.Today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age))
            age--;
            
        return age >= 13 && age <= 120;
    }
}

/// <summary>
/// Validator for UpdatePreferencesRequest.
/// </summary>
public class UpdatePreferencesRequestValidator : AbstractValidator<UpdatePreferencesRequest>
{
    public UpdatePreferencesRequestValidator()
    {
        RuleFor(x => x.PreferredLanguage)
            .Length(2, 10)
            .When(x => !string.IsNullOrEmpty(x.PreferredLanguage))
            .WithMessage("PreferredLanguage must be between 2 and 10 characters");

        RuleFor(x => x.TimeZone)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.TimeZone))
            .WithMessage("TimeZone cannot exceed 50 characters");
    }
}

/// <summary>
/// Validator for UserProfileQueryRequest.
/// </summary>
public class UserProfileQueryRequestValidator : AbstractValidator<UserProfileQueryRequest>
{
    public UserProfileQueryRequestValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.SearchTerm))
            .WithMessage("SearchTerm cannot exceed 100 characters");

        RuleFor(x => x.MinAge)
            .InclusiveBetween(13, 120)
            .When(x => x.MinAge.HasValue)
            .WithMessage("MinAge must be between 13 and 120");

        RuleFor(x => x.MaxAge)
            .InclusiveBetween(13, 120)
            .When(x => x.MaxAge.HasValue)
            .WithMessage("MaxAge must be between 13 and 120");

        RuleFor(x => x)
            .Must(x => !x.MinAge.HasValue || !x.MaxAge.HasValue || x.MinAge <= x.MaxAge)
            .WithMessage("MinAge must be less than or equal to MaxAge");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrEmpty(x.SortBy))
            .WithMessage("Invalid sort field");
    }

    private bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrEmpty(sortBy)) return true;
        
        var validFields = new[] { "name", "createdat", "age", "fitnesslevel" };
        return validFields.Contains(sortBy.ToLower());
    }
}
