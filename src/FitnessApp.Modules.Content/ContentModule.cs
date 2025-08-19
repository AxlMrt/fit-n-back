using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Minio;
using FitnessApp.Modules.Content.Infrastructure.Persistence;
using FitnessApp.Modules.Content.Infrastructure.Repositories;
using FitnessApp.Modules.Content.Application.Interfaces;
using FitnessApp.Modules.Content.Application.Services;
using FitnessApp.Modules.Content.Infrastructure.Storage;
using FitnessApp.Modules.Content.Infrastructure.Workers;

namespace FitnessApp.Modules.Content;

public static class ContentModule
{
    public static IServiceCollection AddContentModule(this IServiceCollection services, string connectionString, IConfiguration configuration)
    {
        services.AddDbContext<ContentDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "content")));

        // Repositories & services
        services.AddScoped<IMediaAssetRepository, MediaAssetRepository>();
        services.AddScoped<IMediaAssetService, MediaAssetService>();

        // Storage (prefer Minio in this deployment)
        services.Configure<MinioSettings>(configuration.GetSection("Minio"));

        // Register Minio client using configured settings and expose MinioStorageService as IStorageService
        services.AddSingleton<IMinioClient>(sp =>
        {
            var opts = sp.GetRequiredService<IOptions<MinioSettings>>().Value;
            return new MinioClient()
                .WithEndpoint(opts.Endpoint)
                .WithCredentials(opts.AccessKey, opts.SecretKey)
                .WithSSL(opts.UseSSL)
                .Build();
        });

        services.AddSingleton<IStorageService, MinioStorageService>();

        // Transcode queue and worker
        var channel = Channel.CreateUnbounded<TranscodeRequest>();
        services.AddSingleton(channel);
        services.AddSingleton<ITranscodeQueue, ChannelTranscodeQueue>();
        services.AddHostedService<BackgroundTranscodeWorker>();

        return services;
    }

    public static IApplicationBuilder UseContentModule(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ContentDbContext>();
        context.Database.Migrate();
        return app;
    }
}
