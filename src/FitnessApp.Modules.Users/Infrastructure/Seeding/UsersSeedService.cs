using FitnessApp.Modules.Authorization.Enums;
using FitnessApp.Modules.Users.Domain.Entities;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Users.Infrastructure.Seeding;

/// <summary>
/// Service for seeding initial users with different roles.
/// </summary>
public class UsersSeedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UsersSeedService> _logger;

    public UsersSeedService(IServiceProvider serviceProvider, ILogger<UsersSeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Seed users with different roles.
    /// </summary>
    public async Task SeedUsersAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

            // Check if database exists, create if not
            await dbContext.Database.EnsureCreatedAsync();

            // Check if we already have users
            if (await dbContext.Users.AnyAsync())
            {
                _logger.LogInformation("User seed skipped: Users already exist in the database.");
                return;
            }

            // Create test users for each role
            var users = new List<User>
            {
                // Admin user
                CreateUser("admin@fitness.app", "admin", Role.Admin, SubscriptionLevel.Elite),
                
                // Coach users
                CreateUser("coach@fitness.app", "coach", Role.Coach, SubscriptionLevel.Premium),
                CreateUser("coach2@fitness.app", "coach2", Role.Coach, SubscriptionLevel.Elite),
                
                // Free Athlete
                CreateUser("free@fitness.app", "free_user", Role.Athlete, SubscriptionLevel.Free),
                
                // Basic Athlete
                CreateUser("basic@fitness.app", "basic_user", Role.Athlete, SubscriptionLevel.Basic),
                
                // Premium Athlete
                CreateUser("premium@fitness.app", "premium_user", Role.Athlete, SubscriptionLevel.Premium),
                
                // Elite Athlete
                CreateUser("elite@fitness.app", "elite_user", Role.Athlete, SubscriptionLevel.Elite)
            };

            // Add users to the database
            await dbContext.Users.AddRangeAsync(users);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("User seed completed successfully. Created {count} test users.", users.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding users.");
            throw;
        }
    }

    private static User CreateUser(string email, string userName, Role role, SubscriptionLevel subscriptionLevel)
    {
        // Create user with the specified role
        var user = new User(email, userName);
        user.SetPasswordHash(BCrypt.Net.BCrypt.HashPassword("Password123!")); // Simple password for testing
        user.SetRole(role);
        user.ConfirmEmail(); // All seeded users have confirmed emails

        // Create user profile
        var profile = new UserProfile(user.Id);
        user.SetProfile(profile);

        // Create subscription
        if (subscriptionLevel != SubscriptionLevel.Free)
        {
            var subscription = new Subscription(
                user,
                subscriptionLevel,
                DateTime.UtcNow,
                DateTime.UtcNow.AddYears(1)
            );
            user.UpdateSubscription(subscription);
        }

        return user;
    }
}
