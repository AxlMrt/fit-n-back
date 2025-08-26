using FitnessApp.Modules.Authentication.Domain.Entities;
using FitnessApp.Modules.Authentication.Domain.Repositories;
using FitnessApp.Modules.Authentication.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Authentication.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for AuthUser entity.
/// Handles only authentication-related data operations.
/// </summary>
public class AuthenticationRepository : IAuthenticationRepository
{
    private readonly AuthenticationDbContext _context;

    public AuthenticationRepository(AuthenticationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<AuthUser?> GetByIdAsync(Guid id)
    {
        return await _context.AuthUsers
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AuthUser?> GetByEmailAsync(string email)
    {
        return await _context.AuthUsers
            .FirstOrDefaultAsync(u => u.Email.Value.ToLower() == email.ToLower());
    }

    public async Task<AuthUser?> GetByUsernameAsync(string username)
    {
        return await _context.AuthUsers
            .FirstOrDefaultAsync(u => u.Username.Value.ToLower() == username.ToLower());
    }

    public async Task<AuthUser> AddAsync(AuthUser authUser)
    {
        _context.AuthUsers.Add(authUser);
        await _context.SaveChangesAsync();
        return authUser;
    }

    public async Task<AuthUser> UpdateAsync(AuthUser authUser)
    {
        _context.AuthUsers.Update(authUser);
        await _context.SaveChangesAsync();
        return authUser;
    }

    public async Task DeleteAsync(Guid id)
    {
        var authUser = await GetByIdAsync(id);
        if (authUser != null)
        {
            _context.AuthUsers.Remove(authUser);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsWithEmailAsync(string email)
    {
        return await _context.AuthUsers
            .AnyAsync(u => u.Email.Value.ToLower() == email.ToLower());
    }

    public async Task<bool> ExistsWithUsernameAsync(string username)
    {
        return await _context.AuthUsers
            .AnyAsync(u => u.Username.Value.ToLower() == username.ToLower());
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.AuthUsers
            .AnyAsync(u => u.Id == id);
    }

    public async Task<AuthUser?> GetByPasswordResetTokenAsync(string token)
    {
        return await _context.AuthUsers
            .FirstOrDefaultAsync(u => u.PasswordResetToken == token && 
                                    u.PasswordResetTokenExpiresAt > DateTime.UtcNow);
    }

    public async Task<AuthUser?> GetByEmailVerificationTokenAsync(string token)
    {
        return await _context.AuthUsers
            .FirstOrDefaultAsync(u => u.EmailVerificationToken == token && 
                                    u.EmailVerificationTokenExpiresAt > DateTime.UtcNow);
    }

    public async Task<IEnumerable<AuthUser>> GetAllAsync()
    {
        return await _context.AuthUsers.ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.AuthUsers.CountAsync();
    }

    public async Task<int> CountActiveAsync()
    {
        return await _context.AuthUsers.CountAsync(u => u.IsActive);
    }

    public async Task<AuthUser?> GetActiveUserByEmailAsync(string email)
    {
        return await _context.AuthUsers
            .FirstOrDefaultAsync(u => u.Email.Value.ToLower() == email.ToLower() && u.IsActive);
    }

    public async Task<List<AuthUser>> GetLockedOutUsersAsync()
    {
        return await _context.AuthUsers
            .Where(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<int> GetFailedLoginAttemptsAsync(Guid userId, TimeSpan timeWindow)
    {
        var user = await GetByIdAsync(userId);
        return user?.AccessFailedCount ?? 0;
    }

    public async Task<List<AuthUser>> GetUsersWithRoleAsync(Role role)
    {
        return await _context.AuthUsers
            .Where(u => u.Role == role)
            .ToListAsync();
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        return await _context.AuthUsers
            .CountAsync(u => u.IsActive);
    }
}
