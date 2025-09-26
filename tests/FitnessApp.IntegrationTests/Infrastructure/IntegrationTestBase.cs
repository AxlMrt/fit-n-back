using Microsoft.Extensions.DependencyInjection;
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

        UsersContext = Scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        WorkoutsContext = Scope.ServiceProvider.GetRequiredService<WorkoutsDbContext>();
        ExercisesContext = Scope.ServiceProvider.GetRequiredService<ExercisesDbContext>();
        TrackingContext = Scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
        ContentContext = Scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        AuthenticationContext = Scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();

        JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public virtual async Task InitializeAsync()
    {
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
    /// Authenticates as a regular user for test requests.
    /// </summary>
    protected async Task AuthenticateAsUserAsync()
    {
        var testUser = AuthenticationHelper.CreateTestUser();
        SetAuthorizationHeader(testUser.Token);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Authenticates as an admin for test requests.
    /// </summary>
    protected async Task AuthenticateAsAdminAsync()
    {
        var testAdmin = AuthenticationHelper.CreateTestAdmin();
        SetAuthorizationHeader(testAdmin.Token);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a JSON POST request.
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
    /// Creates a JSON PUT request.
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
    /// Deserializes an HTTP response to an object.
    /// </summary>
    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
            return default;

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Sets the Authorization header with a Bearer token.
    /// </summary>
    protected void SetAuthorizationHeader(string token)
    {
        Client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Clears the Authorization header.
    /// </summary>
    protected void ClearAuthorizationHeader()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    /// <summary>
    /// Waits for a condition to be true (useful for async events).
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
    /// Finds an entity by key in the database context.
    /// </summary>
    protected async Task<T?> FindEntityAsync<T>(DbContext context, params object[] keyValues) where T : class
    {
        return await context.Set<T>().FindAsync(keyValues);
    }

    /// <summary>
    /// Reloads all tracked entities from the database.
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
    /// Asserts that an entity exists in the database.
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
    /// Asserts that an entity does not exist in the database.
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
    /// Counts the number of entities of a given type.
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
