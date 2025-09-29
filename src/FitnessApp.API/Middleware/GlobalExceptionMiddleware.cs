using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using FluentValidation;
using FitnessApp.API.Infrastructure.Errors;
using FitnessApp.API.Infrastructure.Logging;
using FitnessApp.SharedKernel.Exceptions;
using FitnessApp.Modules.Authentication.Domain.Exceptions;
using FitnessApp.Modules.Exercises.Domain.Exceptions;
using FitnessApp.Modules.Users.Domain.Exceptions;
using FitnessApp.Modules.Workouts.Domain.Exceptions;
using FitnessApp.Modules.Tracking.Domain.Exceptions;

namespace FitnessApp.API.Middleware;

/// <summary>
/// Global exception handling middleware for FitnessApp API.
/// Provides centralized, consistent error handling and response formatting.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            // Add trace ID for correlation
            var traceId = context.TraceIdentifier;
            
            // Log with appropriate level based on exception type
            LogException(exception, context, traceId);
            
            await HandleExceptionAsync(context, exception, traceId);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception, string traceId)
    {
        var errorResponse = exception switch
        {
            // Validation Errors
            ValidationException validationEx => CreateValidationErrorResponse(validationEx, traceId),
            
            // Domain Exceptions - Business Rule Violations with proper HTTP status mapping
            DomainException domainEx => CreateErrorResponse(
                GetHttpStatusForDomainException(domainEx),
                domainEx.ErrorCode, 
                domainEx.Message,
                traceId: traceId),
            
            // Resource Not Found
            ResourceNotFoundException resourceEx => CreateErrorResponse(
                HttpStatusCode.NotFound,
                "RESOURCE_NOT_FOUND", 
                resourceEx.Message,
                details: new { ResourceType = resourceEx.ResourceType, ResourceId = resourceEx.ResourceId },
                traceId: traceId),
                
            // Resource Conflict
            ResourceConflictException conflictEx => CreateErrorResponse(
                HttpStatusCode.Conflict,
                "RESOURCE_CONFLICT",
                conflictEx.Message,
                details: new { ResourceType = conflictEx.ResourceType, ConflictReason = conflictEx.ConflictReason },
                traceId: traceId),
            
            // System Exceptions
            UnauthorizedAccessException => CreateErrorResponse(
                HttpStatusCode.Unauthorized, "UNAUTHORIZED_ACCESS", "Access denied", traceId: traceId),
                
            ArgumentException argEx => CreateErrorResponse(
                HttpStatusCode.BadRequest, "INVALID_ARGUMENT", argEx.Message, traceId: traceId),
                
            InvalidOperationException opEx => CreateErrorResponse(
                HttpStatusCode.BadRequest, "INVALID_OPERATION", opEx.Message, traceId: traceId),
            
            // Default for unhandled exceptions
            _ => CreateErrorResponse(
                HttpStatusCode.InternalServerError, 
                "INTERNAL_SERVER_ERROR", 
                _env.IsDevelopment() ? exception.Message : "An internal server error occurred",
                traceId: traceId)
        };

        context.Response.StatusCode = (int)errorResponse.Status;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _env.IsDevelopment()
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private void LogException(Exception exception, HttpContext context, string traceId)
    {
        var requestPath = context.Request.Path;
        var userId = context.User?.Identity?.Name ?? "Anonymous";

        switch (exception)
        {
            case DomainException domainEx:
                _logger.LogDomainException(exception, domainEx.Domain, requestPath, new { TraceId = traceId, UserId = userId });
                break;
                
            case ValidationException validationEx:
                var validationErrors = validationEx.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
                _logger.LogValidationFailure(requestPath, validationErrors, new { TraceId = traceId, UserId = userId });
                break;
                
            case UnauthorizedAccessException:
                _logger.LogUnauthorizedAccess(requestPath, userId, traceId);
                break;
                
            case ResourceNotFoundException resourceEx:
                _logger.LogResourceNotFound(resourceEx.ResourceType, resourceEx.ResourceId, requestPath);
                break;
                
            default:
                _logger.LogError(exception, 
                    "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, UserId: {UserId}",
                    traceId, requestPath, userId);
                break;
        }
    }

    private static ApiErrorResponse CreateErrorResponse(
        HttpStatusCode statusCode,
        string errorCode,
        string message,
        object? details = null,
        string? traceId = null)
    {
        return new ApiErrorResponse(
            Title: GetStatusTitle(statusCode),
            Status: statusCode,
            ErrorCode: errorCode,
            Message: message,
            Details: details,
            Timestamp: DateTime.UtcNow,
            TraceId: traceId
        );
    }

    private static ApiErrorResponse CreateValidationErrorResponse(ValidationException validationException, string traceId)
    {
        var validationErrors = validationException.Errors
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.ErrorMessage).ToArray()
            );

        return new ApiErrorResponse(
            Title: "Validation Failed",
            Status: HttpStatusCode.BadRequest,
            ErrorCode: "VALIDATION_ERROR",
            Message: "One or more validation errors occurred",
            Details: validationErrors,
            Timestamp: DateTime.UtcNow,
            TraceId: traceId
        );
    }

    private static HttpStatusCode GetHttpStatusForDomainException(DomainException domainEx)
    {
        // Map specific error codes to appropriate HTTP status codes
        return domainEx.ErrorCode switch
        {
            // Not Found cases
            "USER_NOT_FOUND" => HttpStatusCode.NotFound,
            "USER_PROFILE_NOT_FOUND" => HttpStatusCode.NotFound,
            "EXERCISE_NOT_FOUND" => HttpStatusCode.NotFound,
            "WORKOUT_NOT_FOUND" => HttpStatusCode.NotFound,
            "PHASE_NOT_FOUND" => HttpStatusCode.NotFound,
            "SESSION_NOT_FOUND" => HttpStatusCode.NotFound,
            "METRIC_NOT_FOUND" => HttpStatusCode.NotFound,
            "WORKOUT_SESSION_NOT_FOUND" => HttpStatusCode.NotFound,
            
            // Conflict cases
            "USER_ALREADY_EXISTS" => HttpStatusCode.Conflict,
            "EMAIL_ALREADY_TAKEN" => HttpStatusCode.Conflict,
            "RESOURCE_ALREADY_EXISTS" => HttpStatusCode.Conflict,
            
            // Unauthorized/Forbidden cases
            "INVALID_CREDENTIALS" => HttpStatusCode.Unauthorized,
            "TOKEN_EXPIRED" => HttpStatusCode.Unauthorized,
            "ACCESS_DENIED" => HttpStatusCode.Forbidden,
            "INSUFFICIENT_PERMISSIONS" => HttpStatusCode.Forbidden,
            
            // Default to BadRequest for business rule violations
            _ => HttpStatusCode.BadRequest
        };
    }

    private static string GetStatusTitle(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "Bad Request",
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.Forbidden => "Forbidden", 
        HttpStatusCode.NotFound => "Not Found",
        HttpStatusCode.Conflict => "Conflict",
        HttpStatusCode.UnprocessableEntity => "Unprocessable Entity",
        HttpStatusCode.InternalServerError => "Internal Server Error",
        _ => statusCode.ToString()
    };
}
