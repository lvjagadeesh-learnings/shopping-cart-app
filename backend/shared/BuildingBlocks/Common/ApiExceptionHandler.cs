using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Common;

/// <summary>Translates <see cref="ApiException"/> subclasses (and unhandled exceptions) into RFC 7807 ProblemDetails.</summary>
public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var statusCode = exception is ApiException apiException ? apiException.StatusCode : 500;

        if (statusCode == 500)
        {
            // Strip CR/LF from user-controlled request data to prevent log forging (CWE-117).
            var method = SanitizeForLog(httpContext.Request.Method);
            var path = SanitizeForLog(httpContext.Request.Path.Value);
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", method, path);
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title = statusCode == 500 ? "An unexpected error occurred." : exception.Message,
            status = statusCode,
            traceId = httpContext.TraceIdentifier
        }, cancellationToken);

        return true;
    }

    private static string SanitizeForLog(string? value) =>
        value?.Replace("\r", string.Empty).Replace("\n", string.Empty) ?? string.Empty;
}

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddApiExceptionHandling(this IServiceCollection services)
    {
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }
}
