using FitnessApp.Modules.Tracking.Domain.Exceptions;
using FitnessApp.SharedKernel.Enums;

namespace FitnessApp.Modules.Tracking.Domain.Entities;

/// <summary>
/// Represents a user metric recorded over time (weight, strength records, etc.)
/// </summary>
public class UserMetric
{
    private UserMetric() { } // For EF Core

    public UserMetric(
        Guid userId,
        UserMetricType metricType,
        double value,
        DateTime? recordedAt = null,
        string? notes = null,
        string? unit = null)
    {
        if (userId == Guid.Empty)
            throw new TrackingDomainException("User ID cannot be empty");

        if (value < 0)
            throw new TrackingDomainException("Metric value cannot be negative");

        Id = Guid.NewGuid();
        UserId = userId;
        MetricType = metricType;
        Value = value;
        RecordedAt = recordedAt ?? DateTime.UtcNow;
        Notes = notes?.Trim();
        Unit = unit?.Trim() ?? GetDefaultUnit(metricType);
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public UserMetricType MetricType { get; private set; }
    public double Value { get; private set; }
    public string Unit { get; private set; } = string.Empty;
    public DateTime RecordedAt { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    #region Update Methods

    /// <summary>
    /// Update the metric value
    /// </summary>
    public void UpdateValue(double newValue, string? notes = null)
    {
        if (newValue < 0)
            throw new TrackingDomainException("Metric value cannot be negative");

        Value = newValue;
        Notes = notes?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update the recorded date
    /// </summary>
    public void UpdateRecordedAt(DateTime recordedAt)
    {
        RecordedAt = recordedAt;
        UpdatedAt = DateTime.UtcNow;
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get default unit for a metric type
    /// </summary>
    private static string GetDefaultUnit(UserMetricType metricType)
    {
        return metricType switch
        {
            UserMetricType.Weight => "kg",
            UserMetricType.Height => "cm", 
            UserMetricType.PersonalRecord => "kg",
            _ => "unit"
        };
    }

    /// <summary>
    /// Get display string for the metric
    /// </summary>
    public string GetDisplayValue()
    {
        return $"{Value:F1} {Unit}";
    }

    /// <summary>
    /// Check if this metric is a personal record (higher is better)
    /// </summary>
    public bool IsPersonalRecordType()
    {
        return MetricType switch
        {
            UserMetricType.PersonalRecord => true,
            UserMetricType.Height => true, // Taller is generally considered better for tracking
            _ => false
        };
    }

    /// <summary>
    /// Check if this metric is a health metric (lower is often better for goals)
    /// </summary>
    public bool IsHealthMetricType()
    {
        return MetricType switch
        {
            UserMetricType.Weight => true, // Often goal is to lose weight
            _ => false
        };
    }

    #endregion
}
