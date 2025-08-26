using FitnessApp.Modules.Authentication.Application.Interfaces;

namespace FitnessApp.Modules.Authentication.Application.Services;


/// <summary>
/// BCrypt-based implementation of password hashing service.
/// Uses work factor of 12 for strong security.
/// </summary>
public class BCryptPasswordHashService : IPasswordHashService
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));
            
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;
            
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }

    public bool NeedsRehash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return true;
            
        try
        {
            return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, WorkFactor);
        }
        catch
        {
            return true;
        }
    }
}
