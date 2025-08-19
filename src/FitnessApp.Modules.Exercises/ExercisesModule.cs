using FitnessApp.Modules.Exercises.Application.Interfaces;
using FitnessApp.Modules.Exercises.Application.Mapping;
using FitnessApp.Modules.Exercises.Application.Services;
using FitnessApp.Modules.Exercises.Domain.Repositories;
using FitnessApp.Modules.Exercises.Infrastructure.Persistence;
using FitnessApp.Modules.Exercises.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace FitnessApp.Modules.Exercises;

public static class ExercisesModule
{
    public static IServiceCollection AddExercisesModule(this IServiceCollection services, string connectionString)
    {
        // Register DbContext
        services.AddDbContext<ExercisesDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "exercises")));

        // Register repositories
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IMuscleGroupRepository, MuscleGroupRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();

        // Register services
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IMuscleGroupService, MuscleGroupService>();
        services.AddScoped<IEquipmentService, EquipmentService>();

        // Register validators
        //services.AddValidatorsFromAssemblyContaining<ExerciseValidator>();

        // Register mappers
        services.AddScoped<IExerciseMapper, ExerciseMapper>();

        return services;
    }
        
    public static IApplicationBuilder UseExercisesModule(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ExercisesDbContext>();
        context.Database.Migrate();
        
        return app;
    }
}