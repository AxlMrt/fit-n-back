using FluentValidation;
using FitnessApp.Modules.Users.Application.DTOs.Requests;

namespace FitnessApp.Modules.Users.Application.Validators;

public class PreferencesUpdateRequestValidator : AbstractValidator<PreferencesUpdateRequest>
{
    public PreferencesUpdateRequestValidator()
    {
        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(50).WithMessage("Category cannot exceed 50 characters")
                .Matches("^[^<>]*$").WithMessage("Category contains invalid characters.");

            items.RuleFor(i => i.Key)
                .NotEmpty().WithMessage("Key is required")
                .MaximumLength(50).WithMessage("Key cannot exceed 50 characters")
                .Matches("^[^<>]*$").WithMessage("Key contains invalid characters.");

            items.RuleFor(i => i.Value)
                .NotEmpty().WithMessage("Value is required")
                .MaximumLength(255).WithMessage("Value cannot exceed 255 characters")
                .Matches("^[^<>]*$").WithMessage("Value contains invalid characters.");
        });
    }
}
