namespace FitnessApp.Modules.Users.Application.Exceptions;

/// <summary>
/// Exception thrown when a user is not found.
/// </summary>
public class UserNotFoundException : Exception
{
    public UserNotFoundException(string message) : base(message)
    {
    }

    public UserNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
