namespace FitnessApp.Modules.Authentication.Application.Interfaces;

public interface IGenerateJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="userName">The username of the user.</param>
    /// <param name="email">The email address of the user.</param>
    /// <returns>A string representing the generated JWT token.</returns>
    string GenerateJwtToken(Guid userId, string userName, string email);
}