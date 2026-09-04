namespace BuildingBlocks.Common;

/// <summary>Base type for exceptions that should map to a specific HTTP status + ProblemDetails response.</summary>
public abstract class ApiException(string message, int statusCode) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

public sealed class NotFoundException(string message) : ApiException(message, StatusCodes.Status404NotFound);

public sealed class ConflictException(string message) : ApiException(message, StatusCodes.Status409Conflict);

public sealed class ValidationApiException(string message) : ApiException(message, StatusCodes.Status400BadRequest);

public sealed class UnauthorizedApiException(string message) : ApiException(message, StatusCodes.Status401Unauthorized);

file static class StatusCodes
{
    public const int Status400BadRequest = 400;
    public const int Status401Unauthorized = 401;
    public const int Status404NotFound = 404;
    public const int Status409Conflict = 409;
}
