using FitnessApp.API.Middleware;

namespace FitnessApp.API.Extensions;

/// <summary>
/// Extension methods for configuring error handling middleware.
/// </summary>
public static class ErrorHandlingExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the application pipeline.
    /// Should be added early in the pipeline to catch all exceptions.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
