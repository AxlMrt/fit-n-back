using FitnessApp.IntegrationTests.Infrastructure;
using FitnessApp.SharedKernel.Events.Users;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessApp.IntegrationTests.Tests.CrossModule;

/// <summary>
/// Tests pour vérifier que MediatR propage bien les événements entre modules
/// </summary>
public class MediatREventPropagationTests : IntegrationTestBase
{
    public MediatREventPropagationTests(TestWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public void MediatR_ShouldBeRegisteredAndAccessible()
    {
        // Arrange & Act
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetService<IMediator>();

        // Assert
        mediator.Should().NotBeNull("MediatR should be registered in DI");
    }

    [Fact]
    public async Task PublishPhysicalMeasurementsEvent_ShouldBeHandleable()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var measurementsEvent = new PhysicalMeasurementsUpdatedEvent(
            userId: Guid.NewGuid(),
            height: 180.0m,
            heightUnit: "cm",
            weight: 75.0m,
            weightUnit: "kg",
            updatedAt: DateTime.UtcNow,
            source: "Test"
        );

        // Act - This will test if the event can be published without exceptions
        // and if any handlers exist
        var publishTask = async () => await mediator.Publish(measurementsEvent, CancellationToken.None);

        // Assert - Should not throw exception
        await publishTask.Should().NotThrowAsync("Publishing events should not fail even if no handlers exist");
    }

    [Fact] 
    public void CheckTrackingHandlerRegistration_ShouldExist()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        // Act - Check if handler is registered
        var handlers = services.GetServices<INotificationHandler<PhysicalMeasurementsUpdatedEvent>>();
        
        // Assert 
        handlers.Should().NotBeNull("PhysicalMeasurementsUpdatedEvent handlers should be registered");
        handlers.Should().NotBeEmpty("At least one handler should exist for PhysicalMeasurementsUpdatedEvent");
        
        // If this passes, we know handlers exist, if it fails, we know registration is the problem
    }
}
