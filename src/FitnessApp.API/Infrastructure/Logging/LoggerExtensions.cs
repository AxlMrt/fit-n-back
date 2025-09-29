using System.Runtime.CompilerServices;

namespace FitnessApp.API.Infrastructure.Logging;

/// <summary>
/// Extension methods for structured logging across the FitnessApp API.
/// Provides consistent, contextual logging for better observability.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// Logs domain exception with structured context.
    /// </summary>
    public static void LogDomainException(
        this ILogger logger, 
        Exception exception, 
        string domain,
        string operation,
        object? context = null)
    {
        logger.LogWarning(exception,
            "Domain exception in {Domain} during {Operation}. Context: {@Context}",
            domain, operation, context);
    }
    
    /// <summary>
    /// Logs successful operation with performance metrics.
    /// </summary>
    public static void LogOperationSuccess(
        this ILogger logger,
        string operation,
        TimeSpan duration,
        object? context = null)
    {
        logger.LogInformation(
            "Operation {Operation} completed successfully in {Duration}ms. Context: {@Context}",
            operation, duration.TotalMilliseconds, context);
    }
    
    /// <summary>
    /// Logs validation failure with details.
    /// </summary>
    public static void LogValidationFailure(
        this ILogger logger,
        string operation,
        Dictionary<string, string[]> validationErrors,
        object? context = null)
    {
        logger.LogWarning(
            "Validation failed for {Operation}. Errors: {@ValidationErrors}. Context: {@Context}",
            operation, validationErrors, context);
    }
    
    /// <summary>
    /// Logs unauthorized access attempt.
    /// </summary>
    public static void LogUnauthorizedAccess(
        this ILogger logger,
        string operation,
        string? userId = null,
        string? additionalInfo = null)
    {
        logger.LogWarning(
            "Unauthorized access attempt for {Operation}. UserId: {UserId}. Info: {AdditionalInfo}",
            operation, userId ?? "Unknown", additionalInfo);
    }
    
    /// <summary>
    /// Logs resource not found with context.
    /// </summary>
    public static void LogResourceNotFound(
        this ILogger logger,
        string resourceType,
        string resourceId,
        string operation,
        [CallerMemberName] string memberName = "")
    {
        logger.LogInformation(
            "Resource not found: {ResourceType} with ID {ResourceId} during {Operation} in {MemberName}",
            resourceType, resourceId, operation, memberName);
    }
}
