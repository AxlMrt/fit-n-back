namespace FitnessApp.Modules.Authentication.Application.Interfaces;

/// <summary>
/// Service for secure password hashing and verification operations.
/// Encapsulates password security best practices.
/// </summary>
public interface IPasswordHashService
{
    /// <summary>
    /// Hashes a plain text password using secure algorithms.
    /// </summary>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies a plain text password against a stored hash.
    /// </summary>
    bool VerifyPassword(string password, string hash);
    
    /// <summary>
    /// Checks if a password hash needs to be rehashed (e.g., cost factor changed).
    /// </summary>
    bool NeedsRehash(string hash);
}
