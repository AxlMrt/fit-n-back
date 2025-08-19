namespace FitnessApp.Modules.Content.Infrastructure.Storage;

public class MinioSettings
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string BucketName { get; set; } = "fitness-media";
    public bool UseSSL { get; set; } = false;
}
