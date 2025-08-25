namespace FitnessApp.Modules.Users.Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated in the User domain.
/// </summary>
public class UserDomainException : Exception
{
    public UserDomainException(string message) : base(message)
    {
    }

    public UserDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
