using FitnessApp.Modules.Authentication.Domain.Entities;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Authentication.Domain.Repositories;

/// <summary>
/// Repository contract for authentication user data access.
/// Focused solely on authentication-related operations.
/// </summary>
public interface IAuthenticationRepository
{
    // Basic CRUD
    Task<AuthUser?> GetByIdAsync(Guid userId);
    Task<AuthUser?> GetByEmailAsync(string email);
    Task<AuthUser?> GetByUsernameAsync(string username);
    Task<AuthUser> AddAsync(AuthUser user);
    Task<AuthUser> UpdateAsync(AuthUser user);
    Task DeleteAsync(Guid userId);
    
    // Existence checks
    Task<bool> ExistsAsync(Guid userId);
    Task<bool> ExistsWithEmailAsync(string email);
    Task<bool> ExistsWithUsernameAsync(string username);
    
    // Authentication-specific queries
    Task<AuthUser?> GetActiveUserByEmailAsync(string email);
    Task<List<AuthUser>> GetLockedOutUsersAsync();
    Task<int> GetFailedLoginAttemptsAsync(Guid userId, TimeSpan timeWindow);
    
    // Token-based queries
    Task<AuthUser?> GetByPasswordResetTokenAsync(string token);
    Task<AuthUser?> GetByEmailVerificationTokenAsync(string token);
    
    // Admin operations
    Task<List<AuthUser>> GetUsersWithRoleAsync(Role role);
    Task<int> GetActiveUsersCountAsync();
}
