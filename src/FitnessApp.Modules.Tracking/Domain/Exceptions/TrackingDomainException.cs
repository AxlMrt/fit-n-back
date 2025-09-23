namespace FitnessApp.Modules.Tracking.Domain.Exceptions;

/// <summary>
/// Domain exception specific to tracking operations
/// </summary>
public class TrackingDomainException : Exception
{
    public TrackingDomainException(string message) : base(message) { }
    
    public TrackingDomainException(string message, Exception innerException) 
        : base(message, innerException) { }
}
