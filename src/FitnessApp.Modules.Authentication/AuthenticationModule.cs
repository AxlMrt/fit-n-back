using System.Reflection;
using FitnessApp.Modules.Authentication.Application.Interfaces;
using FitnessApp.Modules.Authentication.Application.Services;
using FitnessApp.Modules.Authentication.Infrastructure.Persistence;
using FitnessApp.Modules.Authentication.Infrastructure.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using FitnessApp.SharedKernel.Interfaces;
using FitnessApp.SharedKernel.Services;

namespace FitnessApp.Modules.Authentication;

public static class AuthenticationModule
{
    public static IServiceCollection AddAuthenticationModule(this IServiceCollection services, string connectionString)
    {
        // DbContext for auth schema
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "auth"))
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        // Auth service
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRefreshTokenService, PostgresRefreshTokenService>();
        
        // Use existing Users module services for now via fully-qualified interfaces
        services.AddScoped<IValidationService, ValidationService>();
        services.AddScoped<Users.Domain.Repositories.IUserRepository, Users.Infrastructure.Repositories.UserRepository>();
        services.AddScoped<IGenerateJwtTokenService, GenerateJwtTokenService>();
        services.AddScoped<ITokenRevocationService, TokenRevocationService>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        return services;
    }

    public static IApplicationBuilder UseAuthenticationModule(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        context.Database.Migrate();
        return app;
    }
}
