using System.Text.Json;
using OrderService.Api.Contracts;

namespace OrderService.Api.Clients;

/// <summary>
/// Calls Payment Service's internal authorize endpoint used during checkout orchestration. Base
/// address is resolved via Aspire service discovery ("http://payment-service"); configure
/// "Services:PaymentService:BaseUrl" to override in non-Aspire environments.
/// </summary>
public sealed class PaymentServiceClient(HttpClient httpClient) : IPaymentServiceClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<PaymentResult> AuthorizeAsync(Guid orderId, decimal amount, string currency, CancellationToken ct)
    {
        using var response = await httpClient.PostAsJsonAsync(
            "/api/payments/authorize", new AuthorizePaymentRequest(orderId, amount, currency), ct);

        if (!response.IsSuccessStatusCode)
        {
            throw await DownstreamErrorMapper.ToApiExceptionAsync(response, ct);
        }

        return await response.Content.ReadFromJsonAsync<PaymentResult>(JsonOptions, ct)
            ?? throw new InvalidOperationException("Payment Service returned an empty response.");
    }
}
