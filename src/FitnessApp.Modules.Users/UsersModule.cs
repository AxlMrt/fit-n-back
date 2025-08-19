using System.Reflection;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.Modules.Users.Application.Services;
using FitnessApp.Modules.Users.Domain.Repositories;
using FitnessApp.Modules.Users.Infrastructure.Persistence;
using FitnessApp.Modules.Users.Infrastructure.Repositories;
using FitnessApp.SharedKernel.Interfaces;
using FitnessApp.SharedKernel.Services;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessApp.Modules.Users;

public static class UsersModule
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(connectionString, 
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "users")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IValidationService, ValidationService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddDistributedMemoryCache();

        return services;
    }

    public static IApplicationBuilder UseUsersModule(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        context.Database.EnsureCreated();

        return app;
    }
}