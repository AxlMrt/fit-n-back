using FitnessApp.Modules.Users;
using FitnessApp.Modules.Exercises;
using FitnessApp.Modules.Authentication;
using FitnessApp.Modules.Content;

namespace FitnessApp.API.Extensions;
public static class ModuleExtensions
{
    public static IServiceCollection RegisterModules(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddExercisesModule(connectionString!);
        services.AddUsersModule(connectionString!);
        services.AddAuthenticationModule(connectionString!);
        services.AddContentModule(connectionString!, configuration);
        
        return services;
    }

    public static WebApplication UseModules(this WebApplication app)
    {
        app.UseUsersModule();
        app.UseAuthenticationModule();
        app.UseExercisesModule();
        app.UseContentModule();
        
        return app;
    }
}