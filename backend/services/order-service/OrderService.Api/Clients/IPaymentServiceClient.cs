using OrderService.Api.Contracts;

namespace OrderService.Api.Clients;

public interface IPaymentServiceClient
{
    Task<PaymentResult> AuthorizeAsync(Guid orderId, decimal amount, string currency, CancellationToken ct);
}
