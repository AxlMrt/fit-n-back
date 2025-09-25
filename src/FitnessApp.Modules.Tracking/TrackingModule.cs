using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence;
using FitnessApp.Modules.Tracking.Infrastructure.Mapping;
using FitnessApp.Modules.Tracking.Domain.Repositories;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence.Repositories;
using FitnessApp.Modules.Tracking.Application.Interfaces;
using FitnessApp.Modules.Tracking.Application.Services;
using Microsoft.AspNetCore.Builder;
using System.Reflection;

namespace FitnessApp.Modules.Tracking;

public static class TrackingModule
{
    public static IServiceCollection AddTrackingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        // Register DbContext
        services.AddDbContext<TrackingDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Add AutoMapper
        services.AddAutoMapper(cfg => cfg.AddProfile<TrackingMappingProfile>());

        // Add MediatR for event handling
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Register Repositories
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
        services.AddScoped<IUserMetricRepository, UserMetricRepository>();
        services.AddScoped<IPlannedWorkoutRepository, PlannedWorkoutRepository>();

        // Register Services
        services.AddScoped<ITrackingService, TrackingService>();

        return services;
    }

    public static IApplicationBuilder UseTrackingModule(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TrackingDbContext>();
        context.Database.Migrate();
        return app;
    }
}