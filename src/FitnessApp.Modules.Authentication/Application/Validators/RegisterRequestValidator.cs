using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FluentValidation;

namespace FitnessApp.Modules.Authentication.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Matches("^[^<>]*$").WithMessage("Email contains invalid characters."); 

        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(3)
            .Matches("^[a-zA-Z0-9_]*$").WithMessage("Username can only contain letters, numbers, and underscores.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[!@#$%^&*(),.?\":{}|<>]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwords do not match."); 
    }
}
