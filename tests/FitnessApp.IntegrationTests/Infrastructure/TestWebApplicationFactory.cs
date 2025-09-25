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
            // Remplacer la configuration de base de données
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["ConnectionStrings:UsersConnection"] = ConnectionString,
                ["ConnectionStrings:WorkoutsConnection"] = ConnectionString,
                ["ConnectionStrings:ExercisesConnection"] = ConnectionString,
                ["ConnectionStrings:TrackingConnection"] = ConnectionString,
                ["ConnectionStrings:ContentConnection"] = ConnectionString,
                ["ConnectionStrings:AuthenticationConnection"] = ConnectionString,
                // Configuration JWT pour les tests - DOIT matcher les tokens générés
                ["Jwt:Key"] = "2rO4vtN20xfKnM7gQLeGOlXXS9WDt5Z8a3bQ1kY2H8E",
                ["Jwt:Issuer"] = "FitnessApp", 
                ["Jwt:Audience"] = "FitnessAppUsers",
                ["Jwt:ExpiresInMinutes"] = "60",
                // Configuration pour les tests
                ["Environment"] = "Testing",
                ["Logging:LogLevel:Default"] = "Warning"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Supprimer tous les DbContexts et services EF Core existants
            RemoveAllDbContextServices(services);

            // Ajouter les DbContexts de test avec PostgreSQL
            services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            services.AddDbContext<WorkoutsDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            services.AddDbContext<ExercisesDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            services.AddDbContext<TrackingDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            services.AddDbContext<ContentDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            services.AddDbContext<AuthenticationDbContext>(options =>
                options.UseNpgsql(ConnectionString));

            // Configuration de test supplémentaire
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
        
        // Créer et migrer toutes les bases de données
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
        // Liste des types DbContext à remplacer
        var dbContextTypes = new[]
        {
            typeof(UsersDbContext),
            typeof(WorkoutsDbContext),
            typeof(ExercisesDbContext),
            typeof(TrackingDbContext),
            typeof(ContentDbContext),
            typeof(AuthenticationDbContext)
        };

        // Supprimer spécifiquement chaque DbContext et ses options
        foreach (var dbContextType in dbContextTypes)
        {
            // Supprimer le DbContext lui-même
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == dbContextType)
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Supprimer DbContextOptions<TContext>
            var optionsType = typeof(DbContextOptions<>).MakeGenericType(dbContextType);
            descriptorsToRemove = services
                .Where(d => d.ServiceType == optionsType)
                .ToList();
            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }
        }

        // Supprimer les options génériques de DbContext si elles existent
        var genericDescriptors = services
            .Where(d => d.ServiceType == typeof(DbContextOptions))
            .ToList();
        foreach (var descriptor in genericDescriptors)
        {
            services.Remove(descriptor);
        }
    }

    /// <summary>
    /// Nettoie toutes les données de test entre les tests
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        try
        {
            // Obtenir toutes les tables utilisateur (exclure les tables système et migrations)
            var rawQuery = context.Database.SqlQueryRaw<string>(@"
                SELECT schemaname || '.' || tablename as table_name
                FROM pg_tables 
                WHERE schemaname NOT IN ('information_schema', 'pg_catalog', 'pg_toast')
                AND tablename != '__EFMigrationsHistory'
                ORDER BY schemaname, tablename");

            var tableNames = await rawQuery.ToListAsync();

            if (!tableNames.Any()) return;

            // Désactiver temporairement les contraintes de clé étrangère
            await context.Database.ExecuteSqlRawAsync("SET session_replication_role = replica;");

            // Nettoyer toutes les tables
            foreach (var tableName in tableNames)
            {
                try
                {
                    var quotedTableName = tableName.Contains('.') 
                        ? string.Join('.', tableName.Split('.').Select(part => $"\"{part}\""))
                        : $"\"{tableName}\"";
                    
                    await context.Database.ExecuteSqlAsync($"TRUNCATE TABLE {quotedTableName} RESTART IDENTITY CASCADE");
                }
                catch
                {
                    // Fallback vers DELETE si TRUNCATE échoue
                    try
                    {
                        var quotedTableName = tableName.Contains('.') 
                            ? string.Join('.', tableName.Split('.').Select(part => $"\"{part}\""))
                            : $"\"{tableName}\"";
                        
                        await context.Database.ExecuteSqlAsync($"DELETE FROM {quotedTableName}");
                    }
                    catch
                    {
                        // Ignorer les erreurs de nettoyage de tables individuelles
                    }
                }
            }

            // Réactiver les contraintes de clé étrangère
            await context.Database.ExecuteSqlRawAsync("SET session_replication_role = DEFAULT;");
        }
        catch
        {
            // Fallback : nettoyer manuellement les tables principales connues
            try
            {
                var mainTables = new[]
                {
                    "users.user_profiles", "users.preferences", 
                    "auth.auth_users", "auth.refresh_tokens",
                    "content.media_assets", "content.exercise_media_assets",
                    "workouts.workouts", "workouts.workout_phases", "workouts.workout_exercises",
                    "tracking.workout_sessions", "tracking.workout_session_exercises", 
                    "tracking.planned_workouts", "tracking.user_metrics",
                    "public.exercises"
                };

                await context.Database.ExecuteSqlRawAsync("SET session_replication_role = replica;");

                foreach (var table in mainTables)
                {
                    try
                    {
                        await context.Database.ExecuteSqlAsync($"TRUNCATE TABLE {table} RESTART IDENTITY CASCADE");
                    }
                    catch
                    {
                        try
                        {
                            await context.Database.ExecuteSqlAsync($"DELETE FROM {table}");
                        }
                        catch
                        {
                            // Ignorer les erreurs de nettoyage
                        }
                    }
                }

                await context.Database.ExecuteSqlRawAsync("SET session_replication_role = DEFAULT;");
            }
            catch
            {
                // En dernier recours, ignorer les erreurs de nettoyage
            }
        }
    }
}
