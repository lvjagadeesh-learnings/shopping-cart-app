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
            logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
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
