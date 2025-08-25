using FluentValidation;
using FitnessApp.Modules.Exercises.Application.DTOs;
using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Services;
using FitnessApp.Modules.Exercises.Application.Validators;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using FitnessApp.Modules.Exercises.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Exercises;

public static class ExercisesModule
{
    public static IServiceCollection AddExercisesModule(this IServiceCollection services, string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("A valid connection string must be provided to register the Exercises module.", nameof(connectionString));

        // Register EF Core DbContext
        services.AddDbContext<ExercisesDbContext>(options =>
            options.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(ExercisesModule).Assembly.FullName)));

        // Domain repositories
        services.AddScoped<IExerciseRepository, ExerciseRepository>();

        // Application services
        services.AddScoped<IExerciseService, ExerciseService>();

        // FluentValidation validators
        services.AddScoped<IValidator<CreateExerciseDto>, CreateExerciseDtoValidator>();
        services.AddScoped<IValidator<UpdateExerciseDto>, UpdateExerciseDtoValidator>();
        services.AddScoped<IValidator<ExerciseQueryDto>, ExerciseQueryDtoValidator>();

        // AutoMapper if you choose to use it later
        // services.AddAutoMapper(typeof(ExerciseMappingProfile));

        return services;
    }
        
    public static IApplicationBuilder UseExercisesModule(this IApplicationBuilder app)
    {
        try
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ExercisesDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ExercisesDbContext>>();
            
            logger.LogInformation("Applying Exercises module migrations...");
            context.Database.Migrate();
            logger.LogInformation("Exercises module migrations applied successfully");
        }
        catch (Exception ex)
        {
            var serviceProvider = app.ApplicationServices;
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ExercisesDbContext>>();
            logger.LogError(ex, "Error applying Exercises module migrations");
            throw;
        }
        
        return app;
    }
}