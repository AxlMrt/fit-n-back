using FluentAssertions;
using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using FitnessApp.Modules.Tracking.Domain.Entities;
using FitnessApp.Modules.Tracking.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FitnessApp.IntegrationTests.Tests.Tracking;

/// <summary>
/// Tests d'intégration pour l'entité UserMetric
/// </summary>
public class TrackingIntegrationTests : IntegrationTestBase
{
    public TrackingIntegrationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task CreateUserMetric_ShouldPersistCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var recordedAt = DateTime.UtcNow.AddDays(-1);
        
        var userMetric = new UserMetric(
            userId,
            UserMetricType.Weight,
            75.5,
            recordedAt,
            "After morning workout",
            "kg"
        );

        // Act
        await TrackingContext.UserMetrics.AddAsync(userMetric);
        await TrackingContext.SaveChangesAsync();

        // Assert
        var savedMetric = await TrackingContext.UserMetrics
            .FirstOrDefaultAsync(m => m.Id == userMetric.Id);

        savedMetric.Should().NotBeNull();
        savedMetric!.UserId.Should().Be(userId);
        savedMetric.MetricType.Should().Be(UserMetricType.Weight);
        savedMetric.Value.Should().Be(75.5);
        savedMetric.Unit.Should().Be("kg");
        savedMetric.RecordedAt.Should().BeCloseTo(recordedAt, TimeSpan.FromSeconds(1));
        savedMetric.Notes.Should().Be("After morning workout");
        savedMetric.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        savedMetric.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public async Task UpdateUserMetric_ShouldModifyExistingData()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userMetric = new UserMetric(
            userId,
            UserMetricType.Height,
            175.0,
            DateTime.UtcNow,
            "Initial measurement",
            "cm"
        );

        await TrackingContext.UserMetrics.AddAsync(userMetric);
        await TrackingContext.SaveChangesAsync();

        // Act
        userMetric.UpdateValue(176.5, "Updated measurement after growth");
        await TrackingContext.SaveChangesAsync();

        // Assert
        await RefreshContextAsync(TrackingContext);
        var updatedMetric = await TrackingContext.UserMetrics
            .FirstAsync(m => m.Id == userMetric.Id);

        updatedMetric.Value.Should().Be(176.5);
        updatedMetric.Notes.Should().Be("Updated measurement after growth");
        updatedMetric.UpdatedAt.Should().NotBeNull();
        updatedMetric.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task GetUserMetrics_ByUserId_ShouldReturnUserMetrics()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var metricsUser1 = new[]
        {
            new UserMetric(userId1, UserMetricType.Weight, 75.0),
            new UserMetric(userId1, UserMetricType.Height, 175.0)
        };

        var metricsUser2 = new[]
        {
            new UserMetric(userId2, UserMetricType.Weight, 80.0)
        };

        await TrackingContext.UserMetrics.AddRangeAsync(metricsUser1);
        await TrackingContext.UserMetrics.AddRangeAsync(metricsUser2);
        await TrackingContext.SaveChangesAsync();

        // Act
        var user1Metrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId1)
            .ToListAsync();

        // Assert
        user1Metrics.Should().HaveCount(2);
        user1Metrics.Should().Contain(m => m.MetricType == UserMetricType.Weight && m.Value == 75.0);
        user1Metrics.Should().Contain(m => m.MetricType == UserMetricType.Height && m.Value == 175.0);
    }

    [Fact]
    public async Task GetUserMetrics_ByMetricType_ShouldFilterCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var metrics = new[]
        {
            new UserMetric(userId, UserMetricType.Weight, 75.0, DateTime.UtcNow.AddDays(-3)),
            new UserMetric(userId, UserMetricType.Weight, 74.5, DateTime.UtcNow.AddDays(-2)),
            new UserMetric(userId, UserMetricType.Weight, 74.0, DateTime.UtcNow.AddDays(-1)),
            new UserMetric(userId, UserMetricType.Height, 175.0, DateTime.UtcNow.AddDays(-1)),
            new UserMetric(userId, UserMetricType.PersonalRecord, 15.5, DateTime.UtcNow)
        };

        await TrackingContext.UserMetrics.AddRangeAsync(metrics);
        await TrackingContext.SaveChangesAsync();

        // Act
        var weightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Weight)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync();

        // Assert
        weightMetrics.Should().HaveCount(3);
        weightMetrics[0].Value.Should().Be(75.0);
        weightMetrics[1].Value.Should().Be(74.5);
        weightMetrics[2].Value.Should().Be(74.0);
    }

    [Fact]
    public async Task GetUserMetrics_InDateRange_ShouldFilterCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow.Date;
        
        var metrics = new[]
        {
            new UserMetric(userId, UserMetricType.Weight, 75.0, baseDate.AddDays(-5)), // Outside range
            new UserMetric(userId, UserMetricType.Weight, 74.8, baseDate.AddDays(-3)), // Within range
            new UserMetric(userId, UserMetricType.Weight, 74.5, baseDate.AddDays(-2)), // Within range
            new UserMetric(userId, UserMetricType.Weight, 74.2, baseDate.AddDays(-1)), // Within range
            new UserMetric(userId, UserMetricType.Weight, 74.0, baseDate.AddDays(1))   // Outside range
        };

        await TrackingContext.UserMetrics.AddRangeAsync(metrics);
        await TrackingContext.SaveChangesAsync();

        var fromDate = baseDate.AddDays(-3);
        var toDate = baseDate.AddDays(-1);

        // Act
        var metricsInRange = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId 
                    && m.MetricType == UserMetricType.Weight 
                    && m.RecordedAt >= fromDate 
                    && m.RecordedAt <= toDate.AddDays(1)) // Include end date
            .OrderBy(m => m.RecordedAt)
            .ToListAsync();

        // Assert
        metricsInRange.Should().HaveCount(3);
        metricsInRange[0].Value.Should().Be(74.8);
        metricsInRange[1].Value.Should().Be(74.5);
        metricsInRange[2].Value.Should().Be(74.2);
    }

    [Fact]
    public async Task CreateUserMetric_WithDifferentUnits_ShouldPersistCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        var weightKg = new UserMetric(userId, UserMetricType.Weight, 75.0, unit: "kg");
        var weightLbs = new UserMetric(userId, UserMetricType.Weight, 165.3, unit: "lbs");
        var heightCm = new UserMetric(userId, UserMetricType.Height, 175.0, unit: "cm");
        var heightIn = new UserMetric(userId, UserMetricType.Height, 68.9, unit: "in");

        // Act
        await TrackingContext.UserMetrics.AddRangeAsync(weightKg, weightLbs, heightCm, heightIn);
        await TrackingContext.SaveChangesAsync();

        // Assert
        var savedMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId)
            .ToListAsync();

        savedMetrics.Should().HaveCount(4);
        
        var kgMetric = savedMetrics.First(m => m.Unit == "kg");
        kgMetric.Value.Should().Be(75.0);
        
        var lbsMetric = savedMetrics.First(m => m.Unit == "lbs");
        lbsMetric.Value.Should().Be(165.3);
        
        var cmMetric = savedMetrics.First(m => m.Unit == "cm");
        cmMetric.Value.Should().Be(175.0);
        
        var inMetric = savedMetrics.First(m => m.Unit == "in");
        inMetric.Value.Should().Be(68.9);
    }

    [Fact]
    public async Task GetLatestMetric_ByType_ShouldReturnMostRecent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow;
        
        var weightMetrics = new[]
        {
            new UserMetric(userId, UserMetricType.Weight, 76.0, baseDate.AddDays(-3)),
            new UserMetric(userId, UserMetricType.Weight, 75.5, baseDate.AddDays(-2)), // Most recent
            new UserMetric(userId, UserMetricType.Weight, 75.8, baseDate.AddDays(-4))
        };

        await TrackingContext.UserMetrics.AddRangeAsync(weightMetrics);
        await TrackingContext.SaveChangesAsync();

        // Act
        var latestWeight = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Weight)
            .OrderByDescending(m => m.RecordedAt)
            .FirstOrDefaultAsync();

        // Assert
        latestWeight.Should().NotBeNull();
        latestWeight!.Value.Should().Be(75.5);
        latestWeight.RecordedAt.Should().BeCloseTo(baseDate.AddDays(-2), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateUserMetric_WithInvalidUserId_ShouldThrowException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<TrackingDomainException>(() =>
        {
            var metric = new UserMetric(
                Guid.Empty, // Invalid user ID
                UserMetricType.Weight,
                75.0
            );
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CreateUserMetric_WithNegativeValue_ShouldThrowException()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<TrackingDomainException>(() =>
        {
            var metric = new UserMetric(
                Guid.NewGuid(),
                UserMetricType.Weight,
                -10.0 // Negative value
            );
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task UpdateMetricValue_WithNegativeValue_ShouldThrowException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var metric = new UserMetric(userId, UserMetricType.Weight, 75.0);
        
        await TrackingContext.UserMetrics.AddAsync(metric);
        await TrackingContext.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<TrackingDomainException>(() =>
        {
            metric.UpdateValue(-5.0); // Negative value
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task GetMetricTrend_ShouldShowProgressOverTime()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var baseDate = DateTime.UtcNow.Date;
        
        var weightProgressMetrics = new[]
        {
            new UserMetric(userId, UserMetricType.Weight, 80.0, baseDate.AddDays(-30), "Starting weight"),
            new UserMetric(userId, UserMetricType.Weight, 78.5, baseDate.AddDays(-20), "Week 1-2 progress"),
            new UserMetric(userId, UserMetricType.Weight, 77.2, baseDate.AddDays(-10), "Week 3 progress"),
            new UserMetric(userId, UserMetricType.Weight, 75.8, baseDate, "Current weight")
        };

        await TrackingContext.UserMetrics.AddRangeAsync(weightProgressMetrics);
        await TrackingContext.SaveChangesAsync();

        // Act
        var weightTrend = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Weight)
            .OrderBy(m => m.RecordedAt)
            .Select(m => new { m.Value, m.RecordedAt, m.Notes })
            .ToListAsync();

        // Assert
        weightTrend.Should().HaveCount(4);
        
        // Verify trend shows weight loss over time
        weightTrend[0].Value.Should().Be(80.0);
        weightTrend[1].Value.Should().Be(78.5);
        weightTrend[2].Value.Should().Be(77.2);
        weightTrend[3].Value.Should().Be(75.8);
        
        // Verify progression (weight is decreasing)
        for (int i = 1; i < weightTrend.Count; i++)
        {
            weightTrend[i].Value.Should().BeLessThan(weightTrend[i - 1].Value);
        }
    }

    [Fact]
    public async Task DeleteUserMetric_ShouldRemoveFromDatabase()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var metric = new UserMetric(
            userId,
            UserMetricType.Weight,
            75.0,
            notes: "To be deleted"
        );

        await TrackingContext.UserMetrics.AddAsync(metric);
        await TrackingContext.SaveChangesAsync();

        var metricId = metric.Id;

        // Act
        TrackingContext.UserMetrics.Remove(metric);
        await TrackingContext.SaveChangesAsync();

        // Assert
        var deletedMetric = await TrackingContext.UserMetrics
            .FirstOrDefaultAsync(m => m.Id == metricId);

        deletedMetric.Should().BeNull();
    }

    [Fact]
    public async Task MultipleUserMetrics_ShouldPersistIndependently()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var metrics = new[]
        {
            new UserMetric(userId, UserMetricType.Weight, 75.0, notes: "Metric 1"),
            new UserMetric(userId, UserMetricType.Height, 175.0, notes: "Metric 2"),
            new UserMetric(userId, UserMetricType.PersonalRecord, 15.5, notes: "Metric 3")
        };

        // Act
        await TrackingContext.UserMetrics.AddRangeAsync(metrics);
        await TrackingContext.SaveChangesAsync();

        // Assert
        var savedMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId)
            .ToListAsync();

        savedMetrics.Should().HaveCount(3);
        savedMetrics.Select(m => m.Id).Should().BeEquivalentTo(metrics.Select(m => m.Id));
        savedMetrics.Select(m => m.Id).Should().OnlyHaveUniqueItems();
        savedMetrics.Should().Contain(m => m.MetricType == UserMetricType.Weight);
        savedMetrics.Should().Contain(m => m.MetricType == UserMetricType.Height);
        savedMetrics.Should().Contain(m => m.MetricType == UserMetricType.PersonalRecord);
    }
}



