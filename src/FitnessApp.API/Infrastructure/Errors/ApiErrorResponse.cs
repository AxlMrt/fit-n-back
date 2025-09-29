using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitnessApp.API.Infrastructure.Errors;

/// <summary>
/// Standardized API error response following RFC 7807 Problem Details for HTTP APIs.
/// Provides consistent error formatting across the entire FitnessApp API.
/// </summary>
public record ApiErrorResponse(
    /// <summary>
    /// A short, human-readable summary of the problem type.
    /// </summary>
    string Title,
    
    /// <summary>
    /// The HTTP status code for this occurrence of the problem.
    /// </summary>
    HttpStatusCode Status,
    
    /// <summary>
    /// Application-specific error code for programmatic handling.
    /// </summary>
    string ErrorCode,
    
    /// <summary>
    /// A human-readable explanation specific to this occurrence of the problem.
    /// </summary>
    string Message,
    
    /// <summary>
    /// Additional details about the error (validation errors, etc.).
    /// </summary>
    object? Details = null,
    
    /// <summary>
    /// When the error occurred (UTC).
    /// </summary>
    DateTime Timestamp = default,
    
    /// <summary>
    /// Unique identifier for this error occurrence (for correlation/tracking).
    /// </summary>
    string? TraceId = null
);

/// <summary>
/// Specialized error response for validation errors with detailed field-level information.
/// </summary>
public sealed record ValidationErrorResponse(
    string Title,
    HttpStatusCode Status,
    string ErrorCode,
    string Message,
    Dictionary<string, string[]> ValidationErrors,
    DateTime Timestamp,
    string? TraceId = null
) : ApiErrorResponse(Title, Status, ErrorCode, Message, ValidationErrors, Timestamp, TraceId);

/// <summary>
/// JSON converter for HttpStatusCode to serialize as integer.
/// </summary>
public sealed class HttpStatusCodeJsonConverter : JsonConverter<HttpStatusCode>
{
    public override HttpStatusCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return (HttpStatusCode)reader.GetInt32();
    }

    public override void Write(Utf8JsonWriter writer, HttpStatusCode value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value);
    }
}
