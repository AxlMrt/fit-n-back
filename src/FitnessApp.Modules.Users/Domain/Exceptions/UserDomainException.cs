namespace FitnessApp.Modules.Users.Domain.Exceptions;

/// <summary>
/// Exception levée lorsqu'une règle métier est violée dans le domaine User.
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
