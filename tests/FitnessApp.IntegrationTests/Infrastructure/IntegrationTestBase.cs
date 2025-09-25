using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using FitnessApp.Modules.Workouts.Infrastructure.Persistence;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence;
using FitnessApp.Modules.Content.Infrastructure.Persistence;
using FitnessApp.Modules.Authentication.Infrastructure.Persistence;
using FitnessApp.IntegrationTests.Helpers;

namespace FitnessApp.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<TestWebApplicationFactory<Program>>, IAsyncLifetime
{
    protected readonly TestWebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;

    protected readonly UsersDbContext UsersContext;
    protected readonly WorkoutsDbContext WorkoutsContext;
    protected readonly ExercisesDbContext ExercisesContext;
    protected readonly TrackingDbContext TrackingContext;
    protected readonly ContentDbContext ContentContext;
    protected readonly AuthenticationDbContext AuthenticationContext;

    protected readonly JsonSerializerOptions JsonOptions;

    protected IntegrationTestBase(TestWebApplicationFactory<Program> factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Scope = factory.Services.CreateScope();

        // Récupérer tous les DbContexts
        UsersContext = Scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        WorkoutsContext = Scope.ServiceProvider.GetRequiredService<WorkoutsDbContext>();
        ExercisesContext = Scope.ServiceProvider.GetRequiredService<ExercisesDbContext>();
        TrackingContext = Scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
        ContentContext = Scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        AuthenticationContext = Scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

        // Configuration JSON pour l'API
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public virtual async Task InitializeAsync()
    {
        // Nettoyer la base de données avant chaque test
        await Factory.CleanDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        Scope?.Dispose();
        Client?.Dispose();
        return Task.CompletedTask;
    }

    #region Helper Methods

    /// <summary>
    /// Authentifie l'utilisateur en tant qu'utilisateur régulier
    /// </summary>
    protected async Task AuthenticateAsUserAsync()
    {
        var testUser = AuthenticationHelper.CreateTestUser();
        SetAuthorizationHeader(testUser.Token);
        await Task.CompletedTask; // Pour cohérence async
    }

    /// <summary>
    /// Authentifie l'utilisateur en tant qu'admin
    /// </summary>
    protected async Task AuthenticateAsAdminAsync()
    {
        var testAdmin = AuthenticationHelper.CreateTestAdmin();
        SetAuthorizationHeader(testAdmin.Token);
        await Task.CompletedTask; // Pour cohérence async
    }

    /// <summary>
    /// Crée une requête HTTP POST avec du contenu JSON
    /// </summary>
    protected HttpRequestMessage CreateJsonPostRequest(string requestUri, object content)
    {
        var json = JsonSerializer.Serialize(content, JsonOptions);
        return new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    /// Crée une requête HTTP PUT avec du contenu JSON
    /// </summary>
    protected HttpRequestMessage CreateJsonPutRequest(string requestUri, object content)
    {
        var json = JsonSerializer.Serialize(content, JsonOptions);
        return new HttpRequestMessage(HttpMethod.Put, requestUri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    /// Désérialise une réponse HTTP en objet
    /// </summary>
    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Ajoute un header d'autorisation avec Bearer token
    /// </summary>
    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Supprime le header d'autorisation
    /// </summary>
    protected void ClearAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Attend qu'une condition soit vraie (utile pour les événements asynchrones)
    /// </summary>
    protected async Task WaitForConditionAsync(Func<Task<bool>> condition, TimeSpan? timeout = null, TimeSpan? pollInterval = null)
    {
        timeout ??= TimeSpan.FromSeconds(10);
        pollInterval ??= TimeSpan.FromMilliseconds(100);

        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < timeout)
        {
            if (await condition())
                return;

            await Task.Delay(pollInterval.Value);
        }

        throw new TimeoutException($"Condition was not met within {timeout}");
    }

    /// <summary>
    /// Vérifie qu'une entité existe dans la base de données
    /// </summary>
    protected async Task<T?> FindEntityAsync<T>(DbContext context, params object[] keyValues) where T : class
    {
        return await context.Set<T>().FindAsync(keyValues);
    }

    /// <summary>
    /// Recharge les données depuis la base de données
    /// </summary>
    protected async Task RefreshContextAsync(DbContext context)
    {
        var entries = context.ChangeTracker.Entries().ToList();
        foreach (var entry in entries)
        {
            await entry.ReloadAsync();
        }
    }

    #endregion

    #region Database Assertions

    /// <summary>
    /// Vérifie qu'une entité existe dans la base de données
    /// </summary>
    protected async Task AssertEntityExistsAsync<T>(DbContext context, params object[] keyValues) where T : class
    {
        var entity = await context.Set<T>().FindAsync(keyValues);
        if (entity == null)
        {
            throw new AssertionException($"Entity of type {typeof(T).Name} with keys [{string.Join(", ", keyValues)}] was not found in database");
        }
    }

    /// <summary>
    /// Vérifie qu'une entité n'existe pas dans la base de données
    /// </summary>
    protected async Task AssertEntityNotExistsAsync<T>(DbContext context, params object[] keyValues) where T : class
    {
        var entity = await context.Set<T>().FindAsync(keyValues);
        if (entity != null)
        {
            throw new AssertionException($"Entity of type {typeof(T).Name} with keys [{string.Join(", ", keyValues)}] should not exist in database");
        }
    }

    /// <summary>
    /// Compte le nombre d'entités d'un type donné
    /// </summary>
    protected async Task<int> CountEntitiesAsync<T>(DbContext context) where T : class
    {
        return await context.Set<T>().CountAsync();
    }

    #endregion
}

public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}
