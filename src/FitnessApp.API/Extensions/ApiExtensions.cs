using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace FitnessApp.API.Extensions;
public static class ApiExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                // Accept enum values as strings (e.g. "Strength")
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                // Be lenient on property name casing from clients
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            });

        // Add MediatR globally to handle cross-module events
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            Assembly.GetExecutingAssembly(),
            AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(x => x.GetName().Name?.Contains("FitnessApp.SharedKernel") == true) ?? Assembly.GetExecutingAssembly()
        ));

        return services;
    }
}