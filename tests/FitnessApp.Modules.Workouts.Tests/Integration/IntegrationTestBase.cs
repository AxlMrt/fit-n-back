using FitnessApp.Modules.Workouts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessApp.Modules.Workouts.Tests.Integration;

/// <summary>
/// Base class for integration tests that need a database context
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected readonly WorkoutsDbContext Context;
    protected readonly IServiceProvider ServiceProvider;

    protected IntegrationTestBase()
    {
        var services = new ServiceCollection();
        
        // Configure in-memory database
        services.AddDbContext<WorkoutsDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        // Add other services as needed
        ServiceProvider = services.BuildServiceProvider();
        Context = ServiceProvider.GetRequiredService<WorkoutsDbContext>();

        // Ensure database is created
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context?.Dispose();
        ServiceProvider?.GetService<IServiceScope>()?.Dispose();
        GC.SuppressFinalize(this);
    }
}
