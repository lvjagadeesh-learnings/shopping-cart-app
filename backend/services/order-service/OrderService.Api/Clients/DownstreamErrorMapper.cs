using System.Text.Json;
using BuildingBlocks.Common;

namespace OrderService.Api.Clients;

/// <summary>Maps a failed internal-service HTTP response (ProblemDetails JSON) to the matching ApiException subtype, preserving the downstream message.</summary>
internal static class DownstreamErrorMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<Exception> ToApiExceptionAsync(HttpResponseMessage response, CancellationToken ct)
    {
        string message;
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsBody>(JsonOptions, ct);
            message = problem?.Title ?? $"Downstream call failed with status {(int)response.StatusCode}.";
        }
        catch
        {
            message = $"Downstream call failed with status {(int)response.StatusCode}.";
        }

        return (int)response.StatusCode switch
        {
            404 => new NotFoundException(message),
            409 => new ConflictException(message),
            400 => new ValidationApiException(message),
            _ => new HttpRequestException(message)
        };
    }

    private sealed record ProblemDetailsBody(string? Title);
}
