using System.Reflection;
using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.Modules.Authentication.Application.Services;
using FitnessApp.Modules.Authentication.Domain.Repositories;
using FitnessApp.Modules.Authentication.Infrastructure.Persistence;
using FitnessApp.SharedKernel.Interfaces;
using FitnessApp.SharedKernel.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using FitnessApp.Modules.Authentication.Infrastructure.Repositories;

namespace FitnessApp.Modules.Authentication;

public static class AuthenticationModule
{
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AuthenticationDbContext>(options =>
            options.UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "auth")));

        // Auth service
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPasswordHashService, BCryptPasswordHashService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
        services.AddScoped<IGenerateJwtTokenService, GenerateJwtTokenService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }

    public static IApplicationBuilder UseAuthenticationModule(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
        context.Database.Migrate();
        return app;
    }
}
