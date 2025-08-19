using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FluentValidation;

namespace FitnessApp.Modules.Authentication.Application.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Matches("^[^<>]*$").WithMessage("Email contains invalid characters.");

        RuleFor(x => x.Token)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Token contains invalid characters."); 

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[!@#$%^&*(),.?\":{}|<>]").WithMessage("Password must contain at least one special character.");
    }
}
