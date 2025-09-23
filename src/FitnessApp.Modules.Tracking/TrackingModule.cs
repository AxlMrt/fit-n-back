using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence;
using FitnessApp.Modules.Tracking.Infrastructure.Mapping;
using FitnessApp.Modules.Tracking.Domain.Repositories;
using FitnessApp.Modules.Tracking.Infrastructure.Persistence.Repositories;
using FitnessApp.Modules.Tracking.Application.Interfaces;
using FitnessApp.Modules.Tracking.Application.Services;

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
            options.UseSqlServer(connectionString));

        // Add AutoMapper
        services.AddAutoMapper(cfg => cfg.AddProfile<TrackingMappingProfile>());

        // Register Repositories
        services.AddScoped<IWorkoutSessionRepository, WorkoutSessionRepository>();
        services.AddScoped<IUserMetricRepository, UserMetricRepository>();
        services.AddScoped<IPlannedWorkoutRepository, PlannedWorkoutRepository>();

        // Register Services
        services.AddScoped<ITrackingService, TrackingService>();

        return services;
    }
}