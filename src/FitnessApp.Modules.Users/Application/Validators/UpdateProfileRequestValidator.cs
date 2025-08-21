using FluentValidation;
using FitnessApp.Modules.Users.Application.DTOs.Requests;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Users.Application.Validators;

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches("^[^<>]*$").WithMessage("First name contains invalid characters.");

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches("^[^<>]*$").WithMessage("Last name contains invalid characters.");

        RuleFor(x => x.DateOfBirth)
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Date of birth cannot be in the future")
            .GreaterThan(DateTime.UtcNow.Date.AddYears(-120)).WithMessage("Date of birth is not valid"); // Validation d'un âge raisonnable

        RuleFor(x => x.Height)
            .GreaterThan(0).When(x => x.Height.HasValue).WithMessage("Height must be greater than 0");

        RuleFor(x => x.Weight)
            .GreaterThan(0).When(x => x.Weight.HasValue).WithMessage("Weight must be greater than 0");

        RuleFor(x => x.FitnessLevel)
            .Must(value => Enum.TryParse<FitnessLevel>(value, true, out _))
            .When(x => !string.IsNullOrEmpty(x.FitnessLevel))
            .Matches("^[^<>]*$").WithMessage("Fitness level contains invalid characters.")
            .WithMessage("Invalid fitness level");

        RuleFor(x => x.FitnessGoal)
            .Must(value => Enum.TryParse<FitnessGoal>(value, true, out _))
            .When(x => !string.IsNullOrEmpty(x.FitnessGoal))
            .Matches("^[^<>]*$").WithMessage("Fitness goal contains invalid characters.")
            .WithMessage("Invalid fitness goal");
    }
}