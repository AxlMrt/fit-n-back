using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using FitnessApp.Modules.Workouts.Infrastructure.Persistence;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence;
using FitnessApp.Modules.Content.Infrastructure.Persistence;
using FitnessApp.Modules.Authentication.Infrastructure.Persistence;

namespace FitnessApp.IntegrationTests.Infrastructure;

public class TestWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime 
    where TProgram : class
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .WithDatabase("fitnessapp_test")
        .WithUsername("test")
        .WithPassword("test")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => _dbContainer.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["ConnectionStrings:UsersConnection"] = ConnectionString,
                ["ConnectionStrings:WorkoutsConnection"] = ConnectionString,
                ["ConnectionStrings:ExercisesConnection"] = ConnectionString,
                ["ConnectionStrings:TrackingConnection"] = ConnectionString,
                ["ConnectionStrings:ContentConnection"] = ConnectionString,
                ["ConnectionStrings:AuthenticationConnection"] = ConnectionString,
                ["Jwt:Key"] = "2rO4vtN20xfKnM7gQLeGOlXXS9WDt5Z8a3bQ1kY2H8E",
                ["Jwt:Issuer"] = "FitnessApp", 
                ["Jwt:Audience"] = "FitnessAppUsers",
                ["Jwt:ExpiresInMinutes"] = "60",
                ["Environment"] = "Testing",
                ["Logging:LogLevel:Default"] = "Warning"
            });
        });

        builder.ConfigureServices(services =>
        {
            RemoveAllDbContextServices(services);

            services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(ConnectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "users")));

            services.AddDbContext<WorkoutsDbContext>(options =>
                options.UseNpgsql(ConnectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "workouts")));

            services.AddDbContext<ExercisesDbContext>(options =>
                options.UseNpgsql(ConnectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "exercises")));

            services.AddDbContext<TrackingDbContext>(options =>
                options.UseNpgsql(ConnectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "tracking")));

            services.AddDbContext<ContentDbContext>(options =>
                options.UseNpgsql(ConnectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "content")));

            services.AddDbContext<AuthenticationDbContext>(options =>
                options.UseNpgsql(ConnectionString, o => o.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

            services.Configure<LoggerFilterOptions>(options =>
            {
                options.MinLevel = LogLevel.Warning;
            });
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        using var scope = Services.CreateScope();
        var contexts = new DbContext[]
        {
            scope.ServiceProvider.GetRequiredService<UsersDbContext>(),
            scope.ServiceProvider.GetRequiredService<WorkoutsDbContext>(),
            scope.ServiceProvider.GetRequiredService<ExercisesDbContext>(),
            scope.ServiceProvider.GetRequiredService<TrackingDbContext>(),
            scope.ServiceProvider.GetRequiredService<ContentDbContext>(),
            scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>()
        };

        // Delete the database once using the first context
        await contexts[0].Database.EnsureDeletedAsync();
        
        // Apply migrations for all contexts
        foreach (var context in contexts)
        {
            await context.Database.MigrateAsync();
        }
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await base.DisposeAsync();
    }

    private static void RemoveAllDbContextServices(IServiceCollection services)
    {
        var dbContextTypes = new[]
        {
            typeof(UsersDbContext),
            typeof(WorkoutsDbContext),
            typeof(ExercisesDbContext),
            typeof(TrackingDbContext),
            typeof(ContentDbContext),
            typeof(AuthenticationDbContext)
        };

        foreach (var dbContextType in dbContextTypes)
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == dbContextType)
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            var optionsType = typeof(DbContextOptions<>).MakeGenericType(dbContextType);
            descriptorsToRemove = services
                .Where(d => d.ServiceType == optionsType)
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
        }

        var genericDescriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions))
            .ToList();
        foreach (var descriptor in genericDescriptors)
        {
            services.Remove(descriptor);
        }
    }

    /// <summary>
    /// Cleans all test data between tests. Simple deletion approach.
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        
        // Clear known tables in order (respecting foreign keys)
        await ClearUsersDataAsync(scope);
        await ClearAuthenticationDataAsync(scope);
        await ClearWorkoutDataAsync(scope);
        await ClearTrackingDataAsync(scope);
        await ClearExerciseDataAsync(scope);
        await ClearContentDataAsync(scope);
    }

    private async Task ClearUsersDataAsync(IServiceScope scope)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            context.Preferences.RemoveRange(context.Preferences);
            context.UserProfiles.RemoveRange(context.UserProfiles);
            await context.SaveChangesAsync();
        }
        catch
        {
            // Ignore if tables don't exist
        }
    }

    private async Task ClearAuthenticationDataAsync(IServiceScope scope)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
            context.RefreshTokens.RemoveRange(context.RefreshTokens);
            context.AuthUsers.RemoveRange(context.AuthUsers);
            await context.SaveChangesAsync();
        }
        catch
        {
            // Ignore if tables don't exist
        }
    }

    private async Task ClearWorkoutDataAsync(IServiceScope scope)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<WorkoutsDbContext>();
            // Clear workout tables in dependency order
            await context.Database.ExecuteSqlAsync($"DELETE FROM workouts.workout_exercises");
            await context.Database.ExecuteSqlAsync($"DELETE FROM workouts.workout_phases");
            await context.Database.ExecuteSqlAsync($"DELETE FROM workouts.workouts");
        }
        catch
        {
            // Ignore if tables don't exist
        }
    }

    private async Task ClearTrackingDataAsync(IServiceScope scope)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
            // Clear tracking tables in dependency order
            await context.Database.ExecuteSqlAsync($"DELETE FROM tracking.\"WorkoutSessionSet\"");
            await context.Database.ExecuteSqlAsync($"DELETE FROM tracking.workout_session_exercises");
            await context.Database.ExecuteSqlAsync($"DELETE FROM tracking.workout_sessions");
            await context.Database.ExecuteSqlAsync($"DELETE FROM tracking.planned_workouts");
            await context.Database.ExecuteSqlAsync($"DELETE FROM tracking.user_metrics");
        }
        catch
        {
            // Ignore if tables don't exist
        }
    }

    private async Task ClearExerciseDataAsync(IServiceScope scope)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ExercisesDbContext>();
            context.Exercises.RemoveRange(context.Exercises);
            await context.SaveChangesAsync();
        }
        catch
        {
            // Ignore if tables don't exist
        }
    }

    private async Task ClearContentDataAsync(IServiceScope scope)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
            // Clear content tables directly using SQL
            await context.Database.ExecuteSqlAsync($"DELETE FROM content.media_assets");
        }
        catch
        {
            // Ignore if tables don't exist
        }
    }
}
