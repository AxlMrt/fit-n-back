namespace FitnessApp.Modules.Content.Infrastructure.Storage;

public interface IStorageService
{
    Task PutObjectAsync(Stream data, string key, string contentType);
    Task<Stream> GetObjectAsync(string key);
    string GetObjectUrl(string key);
}
