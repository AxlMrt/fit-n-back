namespace FitnessApp.SharedKernel.Exceptions;

/// <summary>
/// Base exception for all domain-related business rule violations.
/// Provides consistent behavior and properties across all domain modules.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// The domain module where the exception occurred (e.g., "Users", "Workouts", "Exercises").
    /// </summary>
    public string Domain { get; }
    
    /// <summary>
    /// Application-specific error code for programmatic handling.
    /// </summary>
    public string ErrorCode { get; }

    protected DomainException(string domain, string errorCode, string message) 
        : base(message)
    {
        Domain = domain;
        ErrorCode = errorCode;
    }

    protected DomainException(string domain, string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        Domain = domain;
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class ResourceNotFoundException : Exception
{
    public string ResourceType { get; }
    public string ResourceId { get; }

    public ResourceNotFoundException(string resourceType, string resourceId)
        : base($"{resourceType} with ID '{resourceId}' was not found")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
    
    public ResourceNotFoundException(string resourceType, string resourceId, Exception innerException)
        : base($"{resourceType} with ID '{resourceId}' was not found", innerException)
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

/// <summary>
/// Exception thrown when a resource already exists and conflicts with the current operation.
/// Maps to HTTP 409 Conflict.
/// </summary>
public sealed class ResourceConflictException : Exception
{
    public string ResourceType { get; }
    public string ConflictReason { get; }

    public ResourceConflictException(string resourceType, string conflictReason)
        : base($"{resourceType} conflict: {conflictReason}")
    {
        ResourceType = resourceType;
        ConflictReason = conflictReason;
    }
}
