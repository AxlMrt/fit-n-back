using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel;
using Minio.DataModel.Response;
using FitnessApp.Modules.Content.Infrastructure.Storage;
using FitnessApp.Modules.Content.Application.Services;
using FitnessApp.Modules.Content.Infrastructure.Persistence;
using FitnessApp.Modules.Content.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using FitnessApp.Modules.Content.Application.Interfaces;

namespace FitnessApp.Modules.Content.Tests;

public class MediaAssetServiceTests
{
    private readonly Mock<ILogger<MediaAssetService>> _mockLogger = new Mock<ILogger<MediaAssetService>>();

    [Fact]
    public async Task UploadAsync_should_store_and_return_id_when_minio_available()
    {
        // Arrange
        var settings = Options.Create(new MinioSettings { Endpoint = "localhost:9000", AccessKey = "minio", SecretKey = "minio123", BucketName = "test-bucket", UseSSL = false });

        var mockClient = new Mock<IMinioClient>();
        mockClient.Setup(c => c.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        mockClient.Setup(c => c.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>())).ReturnsAsync((PutObjectResponse?)null);
        mockClient.Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>())).ReturnsAsync((ObjectStat?)null);
        mockClient.Setup(c => c.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>())).ReturnsAsync("http://minio/test");

        var storage = new MinioStorageService(settings, mockClient.Object);

        var repoMock = new Moq.Mock<IMediaAssetRepository>();
        repoMock.Setup(r => r.AddAsync(It.IsAny<MediaAsset>())).Returns(Task.CompletedTask);
        repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var dbOptions = new DbContextOptionsBuilder<ContentDbContext>().UseInMemoryDatabase("content_test_upload").Options;
        var db = new ContentDbContext(dbOptions);

        var service = new MediaAssetService(repoMock.Object, storage, db, _mockLogger.Object);

        var data = Encoding.UTF8.GetBytes("hello world");
        using var ms = new MemoryStream(data);

        // Act
        var id = await service.UploadAsync(ms, "hello.txt", "text/plain", Guid.NewGuid(), "desc");

        // Assert
        Assert.NotEqual(Guid.Empty, id);
        repoMock.Verify(r => r.AddAsync(It.IsAny<MediaAsset>()), Times.Once);
        repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DownloadAsync_should_return_stream_when_asset_exists()
    {
        // Arrange
        var settings = Options.Create(new MinioSettings { Endpoint = "localhost:9000", AccessKey = "minio", SecretKey = "minio123", BucketName = "test-bucket", UseSSL = false });
        var mockClient = new Mock<IMinioClient>();
        mockClient.Setup(c => c.GetObjectAsync(It.IsAny<GetObjectArgs>(), It.IsAny<CancellationToken>())).ReturnsAsync((ObjectStat?)null);
        mockClient.Setup(c => c.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>())).ReturnsAsync("http://minio/test");

        var storage = new MinioStorageService(settings, mockClient.Object);

        var asset = new MediaAsset("key1", "http://minio/key1", "video", "desc", "video/mp4");
        var repoMock = new Moq.Mock<IMediaAssetRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(asset);

        var dbOptions = new DbContextOptionsBuilder<ContentDbContext>().UseInMemoryDatabase("content_test_download").Options;
        var db = new ContentDbContext(dbOptions);

        var service = new MediaAssetService(repoMock.Object, storage, db, _mockLogger.Object);

        // Act
        var stream = await service.DownloadAsync(asset.Id);

        // Assert
        Assert.NotNull(stream);
    }

    [Fact]
    public async Task GetByExerciseIdAsync_should_return_assets()
    {
        // Arrange
        var settings = Options.Create(new MinioSettings { Endpoint = "localhost:9000", AccessKey = "minio", SecretKey = "minio123", BucketName = "test-bucket", UseSSL = false });
        var mockClient = new Mock<IMinioClient>();
        mockClient.Setup(c => c.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>())).ReturnsAsync("http://minio/test");
        var storage = new MinioStorageService(settings, mockClient.Object);

        var exerciseId = Guid.NewGuid();
        var asset = new MediaAsset($"exercises/{exerciseId}/file.mp4", "http://minio/file.mp4", "video", "desc", "video/mp4");
        var repoMock = new Moq.Mock<IMediaAssetRepository>();
        repoMock.Setup(r => r.GetByExerciseIdAsync(exerciseId)).ReturnsAsync(new List<MediaAsset> { asset });

        var dbOptions = new DbContextOptionsBuilder<ContentDbContext>().UseInMemoryDatabase("content_test_list").Options;
        var db = new ContentDbContext(dbOptions);

        var service = new MediaAssetService(repoMock.Object, storage, db, _mockLogger.Object);

        // Act
        var list = await service.GetByExerciseIdAsync(exerciseId);

        // Assert
        Assert.NotNull(list);
        Assert.Single(list);
    }

    [Fact]
    public async Task DownloadAsync_should_throw_when_asset_missing()
    {
        // Arrange
        var settings = Options.Create(new MinioSettings { Endpoint = "localhost:9000", AccessKey = "minio", SecretKey = "minio123", BucketName = "test-bucket", UseSSL = false });
        var mockClient = new Mock<IMinioClient>();
        mockClient.Setup(c => c.PresignedGetObjectAsync(It.IsAny<PresignedGetObjectArgs>())).ReturnsAsync("http://minio/test");
        var storage = new MinioStorageService(settings, mockClient.Object);

        var repoMock = new Moq.Mock<IMediaAssetRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((MediaAsset?)null);

        var dbOptions = new DbContextOptionsBuilder<ContentDbContext>().UseInMemoryDatabase("content_test_missing").Options;
        var db = new ContentDbContext(dbOptions);

        var service = new MediaAssetService(repoMock.Object, storage, db, _mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await service.DownloadAsync(Guid.NewGuid()));
    }
}
