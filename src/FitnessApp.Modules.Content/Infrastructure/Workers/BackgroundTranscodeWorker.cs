using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xabe.FFmpeg;
using FitnessApp.Modules.Content.Infrastructure.Storage;
using Xabe.FFmpeg.Downloader;

namespace FitnessApp.Modules.Content.Infrastructure.Workers;

public class BackgroundTranscodeWorker : BackgroundService
{
    private readonly Channel<TranscodeRequest> _channel;
    private readonly ILogger<BackgroundTranscodeWorker> _logger;
    private readonly IStorageService _storage;

    public BackgroundTranscodeWorker(Channel<TranscodeRequest> channel, ILogger<BackgroundTranscodeWorker> logger, IStorageService storage)
    {
        _channel = channel;
        _logger = logger;
        _storage = storage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Processing transcode for {AssetId}", job.AssetId);

                // Download original
                using var input = await _storage.GetObjectAsync(job.Key);

                // Prepare temp files
                var inputFile = Path.GetTempFileName();
                var outputFile = Path.ChangeExtension(Path.GetTempFileName(), job.TargetFormat);

                using (var fs = File.Create(inputFile))
                {
                    await input.CopyToAsync(fs, stoppingToken);
                }

                // Ensure ffmpeg binaries are present (Xabe.FFmpeg will download if needed)
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

                var mediaInfo = await FFmpeg.GetMediaInfo(inputFile);
                var conversion = FFmpeg.Conversions.New().AddParameter($"-i \"{inputFile}\" \"{outputFile}\"");
                await conversion.Start();

                // Upload output
                await using var outStream = File.OpenRead(outputFile);
                var outKey = Path.ChangeExtension(job.Key, job.TargetFormat);
                await _storage.PutObjectAsync(outStream, outKey, "video/mp4");

                // cleanup
                File.Delete(inputFile);
                File.Delete(outputFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Transcode job failed for {AssetId}", job.AssetId);
            }
        }
    }
}
