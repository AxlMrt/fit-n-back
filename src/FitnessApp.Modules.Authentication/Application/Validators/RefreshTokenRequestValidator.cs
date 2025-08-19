using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FluentValidation;

namespace FitnessApp.Modules.Authentication.Application.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9-_]*$").WithMessage("Refresh token contains invalid characters.");
    }
}
