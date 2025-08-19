using Microsoft.Extensions.Options;
using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;

namespace FitnessApp.Modules.Content.Infrastructure.Storage;

public class FileSystemStorageService : IStorageService, IDisposable
{
    private readonly MinioSettings _settings;
    private readonly string _basePath;

    public FileSystemStorageService(IOptions<MinioSettings> options)
    {
        _settings = options.Value;
        // Use the configured bucket name as a subfolder under the app's media directory
        _basePath = Path.Combine(AppContext.BaseDirectory, "media", _settings.BucketName ?? "fitness-media");
        Directory.CreateDirectory(_basePath);
    }

    public async Task PutObjectAsync(Stream data, string key, string contentType)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        var filePath = Path.Combine(_basePath, key.Replace('/', Path.DirectorySeparatorChar));
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

        data.Position = 0;
        using var fs = File.Create(filePath);
        await data.CopyToAsync(fs);
    }

    public Task<Stream> GetObjectAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        var filePath = Path.Combine(_basePath, key.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(filePath)) throw new FileNotFoundException("Media object not found", filePath);

        Stream fs = File.OpenRead(filePath);
        return Task.FromResult(fs);
    }

    public string GetObjectUrl(string key)
    {
        // For local development we expose media via the API/gateway at /media/{key}
        return $"/media/{key}";
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}

public class MinioStorageService : IStorageService, IDisposable
{
    private readonly MinioSettings _settings;
    private readonly IMinioClient _client;
    private bool _disposed;

    public MinioStorageService(IOptions<MinioSettings> options, IMinioClient client)
    {
        _settings = options.Value;
        _client = client ?? throw new ArgumentNullException(nameof(client));

        // Ensure bucket exists (best-effort)
        try
        {
            EnsureBucketExistsAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // ignore in constrained environments
        }
    }

    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            bool found = await _client.BucketExistsAsync(new BucketExistsArgs().WithBucket(_settings.BucketName));
            if (!found)
            {
                await _client.MakeBucketAsync(new MakeBucketArgs().WithBucket(_settings.BucketName));
            }
        }
        catch (MinioException)
        {
            // swallow for now - bucket creation may fail in constrained environments
        }
    }

    public async Task PutObjectAsync(Stream data, string key, string contentType)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        // Ensure we have a seekable stream and know the length
        using var msData = new MemoryStream();
        data.Position = 0;
        await data.CopyToAsync(msData);
        msData.Position = 0;

        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(key)
            .WithStreamData(msData)
            .WithObjectSize(msData.Length)
            .WithContentType(contentType));
    }

    public async Task<Stream> GetObjectAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentNullException(nameof(key));

        var ms = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_settings.BucketName)
            .WithObject(key)
            .WithCallbackStream(async (stream, token) => { await stream.CopyToAsync(ms, token); }));
        ms.Position = 0;
        return ms;
    }

    public string GetObjectUrl(string key)
    {
        try
        {
            var presignedUrl = _client.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_settings.BucketName)
                .WithObject(key)
                .WithExpiry(7 * 24 * 60 * 60)).GetAwaiter().GetResult();
            return presignedUrl;
        }
        catch
        {
            return $"/media/{key}";
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
