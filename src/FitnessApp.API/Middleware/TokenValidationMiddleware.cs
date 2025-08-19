using FitnessApp.Modules.Authentication.Application.Interfaces;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace FitnessApp.API.Middleware;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITokenRevocationService tokenRevocationService)
    {
        if (IsPublicEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        if (context.Request.Headers.TryGetValue("Authorization", out StringValues authHeader))
        {
            string authHeaderValue = authHeader.ToString();
            if (authHeaderValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                string token = authHeaderValue["Bearer ".Length..].Trim();

                bool isRevoked = await tokenRevocationService.IsTokenRevokedAsync(token);
                if (isRevoked)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked" });
                    return;
                }
            }
        }

        await _next(context);
    }

    private static bool IsPublicEndpoint(PathString path)
    {
        return path.StartsWithSegments("/api/auth/login") ||
               path.StartsWithSegments("/api/auth/register") ||
               path.StartsWithSegments("/api/auth/forgot-password") ||
               path.StartsWithSegments("/api/auth/reset-password") ||
               path.StartsWithSegments("/api/auth/refresh-token") ||
               path.StartsWithSegments("/swagger");
    }
}