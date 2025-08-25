using FluentValidation;
using FitnessApp.Modules.Workouts.Application.DTOs;
using FitnessApp.Modules.Workouts.Application.Interfaces;
using FitnessApp.Modules.Workouts.Application.Services;
using FitnessApp.Modules.Workouts.Application.Validators;
using FitnessApp.Modules.Workouts.Domain.Repositories;
using FitnessApp.Modules.Workouts.Infrastructure.Persistence;
using FitnessApp.Modules.Workouts.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(WorkoutsModule).Assembly.FullName)));

        // Domain repositories
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();

        // Application services
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IWorkoutAuthorizationService, WorkoutAuthorizationService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        // Add HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();

        // FluentValidation validators
        services.AddScoped<IValidator<CreateWorkoutDto>, CreateWorkoutDtoValidator>();
        services.AddScoped<IValidator<UpdateWorkoutDto>, UpdateWorkoutDtoValidator>();
        services.AddScoped<IValidator<AddWorkoutPhaseDto>, AddWorkoutPhaseDtoValidator>();
        services.AddScoped<IValidator<UpdateWorkoutPhaseDto>, UpdateWorkoutPhaseDtoValidator>();
        services.AddScoped<IValidator<AddWorkoutExerciseDto>, AddWorkoutExerciseDtoValidator>();
        services.AddScoped<IValidator<UpdateWorkoutExerciseDto>, UpdateWorkoutExerciseDtoValidator>();
        services.AddScoped<IValidator<WorkoutQueryDto>, WorkoutQueryDtoValidator>();

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
