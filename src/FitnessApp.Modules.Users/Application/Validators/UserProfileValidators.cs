using FluentValidation;
using FitnessApp.SharedKernel.DTOs.Users.Requests;

namespace FitnessApp.Modules.Users.Application.Validators;

public class CreateUserProfileRequestValidator : AbstractValidator<CreateUserProfileRequest>
{
    public CreateUserProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters")
            .Matches("^[a-zA-ZÀ-ÿ\\s'-]+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters")
            .Matches("^[a-zA-ZÀ-ÿ\\s'-]+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeAtLeast13YearsOld).WithMessage("User must be at least 13 years old")
            .Must(BeReasonableAge).WithMessage("Date of birth must be within a reasonable range");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value");

        RuleFor(x => x.HeightCm)
            .InclusiveBetween(50, 250).WithMessage("Height must be between 50 and 250 cm");

        RuleFor(x => x.WeightKg)
            .InclusiveBetween(10, 500).WithMessage("Weight must be between 10 and 500 kg");

        RuleFor(x => x.FitnessLevel)
            .IsInEnum().WithMessage("Invalid fitness level");

        RuleFor(x => x.PrimaryFitnessGoal)
            .IsInEnum().WithMessage("Invalid fitness goal");
    }

    private static bool BeAtLeast13YearsOld(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;
        return age >= 13;
    }

    private static bool BeReasonableAge(DateTime dateOfBirth)
    {
        var age = DateTime.Today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > DateTime.Today.AddYears(-age))
            age--;
        return age <= 120 && dateOfBirth <= DateTime.Today;
    }
}

public class UpdatePersonalInfoRequestValidator : AbstractValidator<UpdatePersonalInfoRequest>
{
    public UpdatePersonalInfoRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters")
            .Matches("^[a-zA-ZÀ-ÿ\\s'-]+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters")
            .Matches("^[a-zA-ZÀ-ÿ\\s'-]+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.DateOfBirth)
            .Must(BeAtLeast13YearsOld).WithMessage("User must be at least 13 years old")
            .Must(BeReasonableAge).WithMessage("Date of birth must be within a reasonable range")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value")
            .When(x => x.Gender.HasValue);
    }

    private static bool BeAtLeast13YearsOld(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;
        var age = DateTime.Today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age))
            age--;
        return age >= 13;
    }

    private static bool BeReasonableAge(DateTime? dateOfBirth)
    {
        if (!dateOfBirth.HasValue) return true;
        var age = DateTime.Today.Year - dateOfBirth.Value.Year;
        if (dateOfBirth.Value.Date > DateTime.Today.AddYears(-age))
            age--;
        return age <= 120 && dateOfBirth.Value <= DateTime.Today;
    }
}

public class UpdatePhysicalMeasurementsRequestValidator : AbstractValidator<UpdatePhysicalMeasurementsRequest>
{
    public UpdatePhysicalMeasurementsRequestValidator()
    {
        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0")
            .LessThan(300).WithMessage("Height must be less than 300 (supports both cm and converted units)")
            .When(x => x.Height.HasValue);

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Weight must be greater than 0")
            .LessThan(1000).WithMessage("Weight must be less than 1000 (supports both kg and lbs)")
            .When(x => x.Weight.HasValue);

        RuleFor(x => x.Units!.HeightUnit)
            .Must(unit => string.IsNullOrEmpty(unit) || FitnessApp.SharedKernel.Services.MeasurementUnitConverter.IsValidHeightUnit(unit))
            .WithMessage("Invalid height unit. Supported: cm, ft, in")
            .When(x => x.Units != null);

        RuleFor(x => x.Units!.WeightUnit)
            .Must(unit => string.IsNullOrEmpty(unit) || FitnessApp.SharedKernel.Services.MeasurementUnitConverter.IsValidWeightUnit(unit))
            .WithMessage("Invalid weight unit. Supported: kg, lbs")
            .When(x => x.Units != null);

        RuleFor(x => x)
            .Must(x => x.Height.HasValue || x.Weight.HasValue)
            .WithMessage("At least one measurement must be provided");
    }
}

public class UpdateFitnessProfileRequestValidator : AbstractValidator<UpdateFitnessProfileRequest>
{
    public UpdateFitnessProfileRequestValidator()
    {
        RuleFor(x => x.FitnessLevel)
            .IsInEnum().WithMessage("Invalid fitness level")
            .When(x => x.FitnessLevel.HasValue);

        RuleFor(x => x.PrimaryFitnessGoal)
            .IsInEnum().WithMessage("Invalid fitness goal")
            .When(x => x.PrimaryFitnessGoal.HasValue);

        RuleFor(x => x)
            .Must(x => x.FitnessLevel.HasValue || x.PrimaryFitnessGoal.HasValue)
            .WithMessage("At least one fitness profile field must be provided");
    }
}

public class CreateOrUpdatePreferenceRequestValidator : AbstractValidator<CreateOrUpdatePreferenceRequest>
{
    public CreateOrUpdatePreferenceRequestValidator()
    {
        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Category must be a valid preference category");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Key is required")
            .Length(1, 100).WithMessage("Key must be between 1 and 100 characters")
            .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Key can only contain letters, numbers, underscores, dots, and hyphens");

        RuleFor(x => x.Value)
            .MaximumLength(1000).WithMessage("Value cannot exceed 1000 characters");
    }
}

public class UpdateSubscriptionRequestValidator : AbstractValidator<UpdateSubscriptionRequest>
{
    public UpdateSubscriptionRequestValidator()
    {
        RuleFor(x => x.Level)
            .IsInEnum().WithMessage("Invalid subscription level");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");
    }
}