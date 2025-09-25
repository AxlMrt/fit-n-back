using FitnessApp.Modules.Tracking.Application.Interfaces;
using FitnessApp.SharedKernel.Enums;
using FitnessApp.SharedKernel.Events.Users;
using FitnessApp.SharedKernel.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FitnessApp.Modules.Tracking.Application.EventHandlers;

/// <summary>
/// Handles physical measurements updates from Users module and syncs to Tracking metrics
/// </summary>
public class PhysicalMeasurementsUpdatedHandler : INotificationHandler<PhysicalMeasurementsUpdatedEvent>
{
    private readonly ITrackingService _trackingService;
    private readonly ILogger<PhysicalMeasurementsUpdatedHandler> _logger;

    public PhysicalMeasurementsUpdatedHandler(
        ITrackingService trackingService,
        ILogger<PhysicalMeasurementsUpdatedHandler> logger)
    {
        _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Handle(PhysicalMeasurementsUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing physical measurements update for user {UserId}", notification.UserId);

        try
        {
            // Sync height if provided
            if (notification.Height.HasValue && !string.IsNullOrEmpty(notification.HeightUnit))
            {
                var heightCm = MeasurementUnitConverter.ConvertHeightToCentimeters(
                    notification.Height.Value,
                    notification.HeightUnit);

                await _trackingService.RecordUserMetricAsync(
                    notification.UserId,
                    UserMetricType.Height,
                    (double)heightCm,
                    notification.UpdatedAt,
                    $"Auto-sync from profile update ({notification.Source})",
                    "cm",
                    cancellationToken);

                _logger.LogInformation("Height synced for user {UserId}: {Height} {Unit} -> {HeightCm} cm", 
                    notification.UserId, notification.Height.Value, notification.HeightUnit, heightCm);
            }

            // Sync weight if provided
            if (notification.Weight.HasValue && !string.IsNullOrEmpty(notification.WeightUnit))
            {
                var weightKg = MeasurementUnitConverter.ConvertWeightToKilograms(
                    notification.Weight.Value,
                    notification.WeightUnit);

                await _trackingService.RecordUserMetricAsync(
                    notification.UserId,
                    UserMetricType.Weight,
                    (double)weightKg,
                    notification.UpdatedAt,
                    $"Auto-sync from profile update ({notification.Source})",
                    "kg",
                    cancellationToken);

                _logger.LogInformation("Weight synced for user {UserId}: {Weight} {Unit} -> {WeightKg} kg", 
                    notification.UserId, notification.Weight.Value, notification.WeightUnit, weightKg);
            }

            _logger.LogInformation("Physical measurements sync completed for user {UserId}", notification.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync physical measurements for user {UserId}", notification.UserId);
            // Note: We don't rethrow to avoid breaking the profile update
            // The sync failure will be logged and can be retried later
        }
    }
}
