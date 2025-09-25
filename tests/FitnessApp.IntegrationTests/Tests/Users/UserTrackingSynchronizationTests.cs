using FluentAssertions;
using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.IntegrationTests.Helpers;
using FitnessApp.Modules.Users.Application.Interfaces;
using FitnessApp.SharedKernel.DTOs.Users.Requests;
using FitnessApp.SharedKernel.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FitnessApp.Modules.Users.Domain.ValueObjects;

namespace FitnessApp.IntegrationTests.Tests.Users;

/// <summary>
/// Tests d'intégration pour la synchronisation automatique entre les modules Users et Tracking
/// via MediatR lors des mises à jour de mesures physiques
/// </summary>
public class UserTrackingSynchronizationTests : IntegrationTestBase
{
    public UserTrackingSynchronizationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_ShouldSyncToTrackingModule()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new FitnessApp.Modules.Users.Domain.Entities.UserProfile(userId);
        
        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        var userProfileService = Scope.ServiceProvider.GetRequiredService<IUserProfileService>();

        // Act - Mettre à jour les mesures physiques via le service (cela devrait déclencher l'événement MediatR)
        var request = new UpdatePhysicalMeasurementsRequest(
            Height: 175m, // 175 cm
            Weight: 75m,  // 75 kg
            Units: new MeasurementUnits("cm", "kg")
        );

        await userProfileService.UpdatePhysicalMeasurementsAsync(userId, request);

        // Wait a bit for the MediatR event to be processed
        await Task.Delay(200);

        // Assert - Vérifier que les métriques ont été synchronisées dans le module Tracking
        var heightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Height)
            .OrderByDescending(m => m.RecordedAt)
            .ToListAsync();

        var weightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Weight)
            .OrderByDescending(m => m.RecordedAt)
            .ToListAsync();

        // Vérifier qu'au moins une métrique de taille a été créée
        heightMetrics.Should().HaveCountGreaterThan(0);
        var latestHeight = heightMetrics.First();
        latestHeight.Value.Should().Be(175.0); // Converti en cm
        latestHeight.Unit.Should().Be("cm");
        latestHeight.Notes.Should().Contain("Auto-sync");

        // Vérifier qu'au moins une métrique de poids a été créée
        weightMetrics.Should().HaveCountGreaterThan(0);
        var latestWeight = weightMetrics.First();
        latestWeight.Value.Should().Be(75.0); // Converti en kg
        latestWeight.Unit.Should().Be("kg");
        latestWeight.Notes.Should().Contain("Auto-sync");
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_OnlyHeight_ShouldSyncOnlyHeight()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new FitnessApp.Modules.Users.Domain.Entities.UserProfile(userId);
        
        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        var userProfileService = Scope.ServiceProvider.GetRequiredService<IUserProfileService>();

        // Act - Mettre à jour uniquement la taille
        var request = new UpdatePhysicalMeasurementsRequest(
            Height: 170m, // Seulement la taille
            Weight: null,
            Units: new MeasurementUnits("cm", "kg")
        );

        await userProfileService.UpdatePhysicalMeasurementsAsync(userId, request);

        // Wait for MediatR processing
        await Task.Delay(200);

        // Assert
        var heightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Height)
            .ToListAsync();

        var weightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Weight)
            .ToListAsync();

        // Devrait avoir une métrique de taille mais pas de poids
        heightMetrics.Should().HaveCountGreaterThan(0);
        weightMetrics.Should().HaveCount(0);

        var heightMetric = heightMetrics.First();
        heightMetric.Value.Should().Be(170.0);
        heightMetric.Unit.Should().Be("cm");
        heightMetric.Notes.Should().Contain("Auto-sync");
    }

    [Fact]
    public async Task UpdatePhysicalMeasurements_OnlyWeight_ShouldSyncOnlyWeight()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new FitnessApp.Modules.Users.Domain.Entities.UserProfile(userId);
        
        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        var userProfileService = Scope.ServiceProvider.GetRequiredService<IUserProfileService>();

        // Act - Mettre à jour uniquement le poids
        var request = new UpdatePhysicalMeasurementsRequest(
            Height: null,
            Weight: 70m, // Seulement le poids
            Units: new MeasurementUnits("cm", "kg")
        );

        await userProfileService.UpdatePhysicalMeasurementsAsync(userId, request);

        // Wait for MediatR processing
        await Task.Delay(200);

        // Assert
        var heightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Height)
            .ToListAsync();

        var weightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Weight)
            .ToListAsync();

        // Devrait avoir une métrique de poids mais pas de taille
        heightMetrics.Should().HaveCount(0);
        weightMetrics.Should().HaveCountGreaterThan(0);

        var weightMetric = weightMetrics.First();
        weightMetric.Value.Should().Be(70.0);
        weightMetric.Unit.Should().Be("kg");
        weightMetric.Notes.Should().Contain("Auto-sync");
    }

    [Fact]
    public async Task MultipleUpdates_ShouldCreateSeparateTrackingEntries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userProfile = new FitnessApp.Modules.Users.Domain.Entities.UserProfile(userId);
        
        await UsersContext.UserProfiles.AddAsync(userProfile);
        await UsersContext.SaveChangesAsync();

        var userProfileService = Scope.ServiceProvider.GetRequiredService<IUserProfileService>();

        // Act - Effectuer plusieurs mises à jour
        var request1 = new UpdatePhysicalMeasurementsRequest(
            Height: 175m,
            Weight: 75m,
            Units: new MeasurementUnits("cm", "kg")
        );

        await userProfileService.UpdatePhysicalMeasurementsAsync(userId, request1);
        await Task.Delay(200); // Attendre le traitement MediatR

        var request2 = new UpdatePhysicalMeasurementsRequest(
            Height: 176m,
            Weight: 74m,
            Units: new MeasurementUnits("cm", "kg")
        );

        await userProfileService.UpdatePhysicalMeasurementsAsync(userId, request2);
        await Task.Delay(200); // Attendre le traitement MediatR

        // Assert
        var heightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Height)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync();

        var weightMetrics = await TrackingContext.UserMetrics
            .Where(m => m.UserId == userId && m.MetricType == UserMetricType.Weight)
            .OrderBy(m => m.RecordedAt)
            .ToListAsync();

        // Devrait avoir au moins 2 entrées pour chaque métrique (une pour chaque mise à jour)
        heightMetrics.Should().HaveCountGreaterOrEqualTo(2);
        weightMetrics.Should().HaveCountGreaterOrEqualTo(2);

        // Vérifier que les valeurs correspondent aux mises à jour
        heightMetrics.Should().Contain(m => m.Value == 175.0);
        heightMetrics.Should().Contain(m => m.Value == 176.0);
        weightMetrics.Should().Contain(m => m.Value == 75.0);
        weightMetrics.Should().Contain(m => m.Value == 74.0);
    }
}



