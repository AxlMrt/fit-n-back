using FitnessApp.Modules.Workouts.Application.Interfaces;
using FitnessApp.Modules.Workouts.Application.Services;
using FitnessApp.Modules.Workouts.Domain.Repositories;
using FitnessApp.Modules.Workouts.Infrastructure.Persistence;
using FitnessApp.Modules.Workouts.Infrastructure.Repositories;
using FitnessApp.Modules.Workouts.Application.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Workouts;

/// <summary>
/// Configuration module for Workouts functionality
/// </summary>
public static class WorkoutsModule
{
    public static IServiceCollection AddWorkoutsModule(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A valid connection string must be provided to register the Workouts module.", nameof(connectionString));

        // Register EF Core DbContext
        services.AddDbContext<WorkoutsDbContext>(options =>
            options.UseNpgsql(connectionString, 
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "workouts")));

        // Domain repositories
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();

        // Application services
        services.AddScoped<IWorkoutService, WorkoutService>();

        // Add HttpContextAccessor
        services.AddHttpContextAccessor();

        // AutoMapper
        services.AddAutoMapper(cfg => cfg.AddProfile<WorkoutMappingProfile>());

        return services;
    }
        
    public static IApplicationBuilder UseWorkoutsModule(this IApplicationBuilder app)
    {   
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WorkoutsDbContext>();
        context.Database.Migrate();
        
        return app;
    }
}
