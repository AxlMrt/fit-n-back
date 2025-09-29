using FitnessApp.SharedKernel.Exceptions;

namespace FitnessApp.Modules.Authentication.Domain.Exceptions;

/// <summary>
/// Domain exception for authentication-related business rule violations.
/// </summary>
public sealed class AuthenticationDomainException : DomainException
{
    public AuthenticationDomainException(string errorCode, string message) 
        : base("Authentication", errorCode, message)
    {
    }

    public AuthenticationDomainException(string errorCode, string message, Exception innerException) 
        : base("Authentication", errorCode, message, innerException)
    {
    }

    // Factory methods for common authentication domain errors
    public static AuthenticationDomainException InvalidCredentials() =>
        new("INVALID_CREDENTIALS", "Invalid email or password");

    public static AuthenticationDomainException AccountLocked(DateTime? lockoutEnd) =>
        new("ACCOUNT_LOCKED", lockoutEnd.HasValue 
            ? $"Account is locked until {lockoutEnd:yyyy-MM-dd HH:mm:ss} UTC"
            : "Account is permanently locked");

    public static AuthenticationDomainException EmailNotConfirmed() =>
        new("EMAIL_NOT_CONFIRMED", "Email address has not been confirmed");

    public static AuthenticationDomainException InvalidToken() =>
        new("INVALID_TOKEN", "The provided token is invalid or expired");

    public static AuthenticationDomainException WeakPassword() =>
        new("WEAK_PASSWORD", "Password does not meet security requirements");

    public static AuthenticationDomainException EmailAlreadyExists(string email) =>
        new("EMAIL_ALREADY_EXISTS", $"An account with email '{email}' already exists");

    public static AuthenticationDomainException UsernameAlreadyExists(string username) =>
        new("USERNAME_ALREADY_EXISTS", $"Username '{username}' is already taken");

    public static AuthenticationDomainException PasswordMismatch() =>
        new("PASSWORD_MISMATCH", "Password and confirmation password do not match");

    public static AuthenticationDomainException UserNotFound() =>
        new("USER_NOT_FOUND", "User not found or inactive");

    public static AuthenticationDomainException IncorrectPassword() =>
        new("INCORRECT_PASSWORD", "The current password is incorrect");

    public static AuthenticationDomainException InvalidPasswordHash() =>
        new("INVALID_PASSWORD_HASH", "Password hash cannot be null");

    public static AuthenticationDomainException LockoutNotEnabled() =>
        new("LOCKOUT_NOT_ENABLED", "Account lockout is not enabled for this user");

    public static AuthenticationDomainException InactiveToken() =>
        new("INACTIVE_TOKEN", "Cannot use an inactive token");

    public static AuthenticationDomainException EmptyPassword() =>
        new("EMPTY_PASSWORD", "Password cannot be empty");

    public static AuthenticationDomainException PasswordTooShort() =>
        new("PASSWORD_TOO_SHORT", "Password must be at least 8 characters long");

    public static AuthenticationDomainException PasswordTooLong() =>
        new("PASSWORD_TOO_LONG", "Password cannot exceed 128 characters");

    public static AuthenticationDomainException EmptyPasswordHash() =>
        new("EMPTY_PASSWORD_HASH", "Password hash cannot be empty");

    public static AuthenticationDomainException InvalidPasswordComplexity(string missingRequirements) =>
        new("INVALID_PASSWORD_COMPLEXITY", $"Password must contain at least one: {missingRequirements}");
}
