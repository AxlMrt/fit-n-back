namespace FitnessApp.Modules.Workouts.Application.Interfaces;

/// <summary>
/// Service to get information about the current user
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Get the current authenticated user's ID
    /// </summary>
    Guid? GetCurrentUserId();

    /// <summary>
    /// Check if there is a current authenticated user
    /// </summary>
    bool IsAuthenticated();

    /// <summary>
    /// Get the current user's claims or roles if needed
    /// </summary>
    IEnumerable<string> GetUserRoles();
}
