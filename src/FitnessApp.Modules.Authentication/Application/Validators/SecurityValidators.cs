using FitnessApp.SharedKernel.DTOs.Auth.Requests;
using FluentValidation;

namespace FitnessApp.Modules.Authentication.Application.Validators;

/// <summary>
/// Centralized password validation rules enforcing security policies.
/// Used across all authentication operations requiring password validation.
/// </summary>
public static class PasswordValidationRules
{
    public const int MinLength = 12;
    public const int MaxLength = 128;
    
    public static IRuleBuilder<T, string> StrongPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Le mot de passe est requis")
            .Length(MinLength, MaxLength).WithMessage($"Le mot de passe doit contenir entre {MinLength} et {MaxLength} caractères")
            .Matches(@"[A-Z]").WithMessage("Le mot de passe doit contenir au moins une majuscule")
            .Matches(@"[a-z]").WithMessage("Le mot de passe doit contenir au moins une minuscule")
            .Matches(@"[0-9]").WithMessage("Le mot de passe doit contenir au moins un chiffre")
            .Matches(@"[!@#$%^&*(),.?\"":{}|<>]").WithMessage("Le mot de passe doit contenir au moins un caractère spécial")
            .Must(NotContainSequentialCharacters).WithMessage("Le mot de passe ne doit pas contenir de caractères séquentiels")
            .Must(NotContainRepeatingCharacters).WithMessage("Le mot de passe ne doit pas contenir trop de caractères répétés");
    }

    public static IRuleBuilder<T, string> SafeEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("L'email est requis")
            .EmailAddress().WithMessage("Format d'email invalide")
            .MaximumLength(320).WithMessage("L'email ne peut pas dépasser 320 caractères")
            .Matches(@"^[^<>""'&]*$").WithMessage("L'email contient des caractères non autorisés");
    }

    public static IRuleBuilder<T, string> SafeUsername<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Le nom d'utilisateur est requis")
            .Length(3, 30).WithMessage("Le nom d'utilisateur doit contenir entre 3 et 30 caractères")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Le nom d'utilisateur ne peut contenir que des lettres, chiffres, points, traits d'union et underscores")
            .Must(NotStartOrEndWithSpecialChar).WithMessage("Le nom d'utilisateur ne peut pas commencer ou finir par un caractère spécial");
    }

    private static bool NotContainSequentialCharacters(string password)
    {
        if (string.IsNullOrEmpty(password)) return true;
        
        var sequences = new[] { "123", "234", "345", "456", "567", "678", "789", "890",
                               "abc", "bcd", "cde", "def", "efg", "fgh", "ghi", "hij", "ijk", 
                               "jkl", "klm", "lmnop", "nop", "opq", "pqr", "qrs", "rst", "stu", "tuv", "uvw", "vwx", "wxy", "xyz" };
        
        return !sequences.Any(seq => password.ToLowerInvariant().Contains(seq));
    }

    private static bool NotContainRepeatingCharacters(string password)
    {
        if (string.IsNullOrEmpty(password)) return true;
        
        int maxRepeats = 3;
        for (int i = 0; i <= password.Length - maxRepeats; i++)
        {
            char currentChar = password[i];
            int count = 1;
            
            for (int j = i + 1; j < password.Length && j < i + maxRepeats; j++)
            {
                if (password[j] == currentChar)
                    count++;
                else
                    break;
            }
            
            if (count >= maxRepeats)
                return false;
        }
        
        return true;
    }

    private static bool NotStartOrEndWithSpecialChar(string username)
    {
        if (string.IsNullOrEmpty(username)) return false;
        
        var specialChars = new[] { '.', '-', '_' };
        return !specialChars.Contains(username[0]) && !specialChars.Contains(username[^1]);
    }
}

/// <summary>
/// Enhanced register request validator with centralized security rules.
/// </summary>
public class EnhancedRegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public EnhancedRegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .SafeEmail();

        RuleFor(x => x.UserName)
            .SafeUsername();

        RuleFor(x => x.Password)
            .StrongPassword();

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmation du mot de passe est requise")
            .Equal(x => x.Password).WithMessage("Les mots de passe ne correspondent pas");
    }
}

/// <summary>
/// Enhanced login request validator with security considerations.
/// </summary>
public class EnhancedLoginRequestValidator : AbstractValidator<LoginRequest>
{
    public EnhancedLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .SafeEmail();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est requis")
            .MaximumLength(128).WithMessage("Mot de passe trop long"); // Prevent DoS attacks
    }
}

/// <summary>
/// Change password request validator ensuring old and new passwords are different.
/// </summary>
public class EnhancedChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public EnhancedChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Le mot de passe actuel est requis");

        RuleFor(x => x.NewPassword)
            .StrongPassword();

        RuleFor(x => x.NewPassword)
            .NotEqual(x => x.CurrentPassword)
            .WithMessage("Le nouveau mot de passe doit être différent de l'actuel");
    }
}
