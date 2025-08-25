using FluentValidation;
using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.SharedKernel.Enums;
using FitnessApp.Modules.Users.Domain.Enums;

namespace FitnessApp.Modules.Users.Application.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(320).WithMessage("Email cannot exceed 320 characters");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 30).WithMessage("Username must be between 3 and 30 characters")
            .Matches(@"^[a-zA-Z0-9._-]{3,30}$")
            .WithMessage("Username can only contain letters, numbers, dots, underscores and hyphens");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number and one special character");

        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches(@"^[^<>&]*$").WithMessage("First name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches(@"^[^<>&]*$").WithMessage("Last name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));
    }
}

public class UpdateUserProfileRequestValidator : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches(@"^[^<>&]*$").WithMessage("First name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches(@"^[^<>&]*$").WithMessage("Last name contains invalid characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Date of birth cannot be in the future")
            .GreaterThan(DateTime.UtcNow.Date.AddYears(-120)).WithMessage("Date of birth indicates an unrealistic age")
            .Must(BeValidAge).WithMessage("Users must be at least 13 years old")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value")
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.HeightCm)
            .InclusiveBetween(50, 250).WithMessage("Height must be between 50cm and 250cm")
            .When(x => x.HeightCm.HasValue);

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(20, 300).WithMessage("Weight must be between 20kg and 300kg")
            .When(x => x.WeightKg.HasValue);

        RuleFor(x => x.FitnessLevel)
            .IsInEnum().WithMessage("Invalid fitness level")
            .When(x => x.FitnessLevel.HasValue);

        RuleFor(x => x.PrimaryFitnessGoal)
            .IsInEnum().WithMessage("Invalid fitness goal")
            .When(x => x.PrimaryFitnessGoal.HasValue);
    }

    private static bool BeValidAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;
        
        var age = DateTime.UtcNow.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > DateTime.UtcNow.Date.AddYears(-age))
            age--;
            
        return age >= 13;
    }
}

public class UpdateUserEmailRequestValidator : AbstractValidator<UpdateUserEmailRequest>
{
    public UpdateUserEmailRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("New email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(320).WithMessage("Email cannot exceed 320 characters");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one number and one special character");

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("New password must be different from current password");
    }
}

public class UserQueryRequestValidator : AbstractValidator<UserQueryRequest>
{
    private static readonly string[] ValidSortFields = 
    {
        "Id", "Email", "Username", "CreatedAt", "UpdatedAt", "LastLoginAt", "Name", "Age"
    };

    public UserQueryRequestValidator()
    {
        RuleFor(x => x.EmailFilter)
            .EmailAddress().WithMessage("Invalid email format for filter")
            .When(x => !string.IsNullOrEmpty(x.EmailFilter));

        RuleFor(x => x.NameFilter)
            .MaximumLength(100).WithMessage("Name filter cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.NameFilter));

        RuleFor(x => x.GenderFilter)
            .IsInEnum().WithMessage("Invalid gender filter")
            .When(x => x.GenderFilter.HasValue);

        RuleFor(x => x.FitnessLevelFilter)
            .IsInEnum().WithMessage("Invalid fitness level filter")
            .When(x => x.FitnessLevelFilter.HasValue);

        RuleFor(x => x.SortBy)
            .NotEmpty().WithMessage("Sort field is required")
            .Must(BeValidSortField).WithMessage($"Sort field must be one of: {string.Join(", ", ValidSortFields)}");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100");
    }

    private static bool BeValidSortField(string sortBy)
    {
        return ValidSortFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }
}
