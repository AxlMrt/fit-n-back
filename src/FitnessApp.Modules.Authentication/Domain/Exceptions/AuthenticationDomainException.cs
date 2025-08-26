namespace FitnessApp.Modules.Authentication.Domain.Exceptions;

/// <summary>
/// Domain exception for authentication-related business rule violations.
/// </summary>
public sealed class AuthenticationDomainException : Exception
{
    public AuthenticationDomainException()
    {
    }

    public AuthenticationDomainException(string message) : base(message)
    {
    }

    public AuthenticationDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
