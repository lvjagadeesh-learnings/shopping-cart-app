using PaymentService.Api.Domain;

namespace PaymentService.Api.Contracts;

internal static class PaymentMapping
{
    public static PaymentResponse ToResponse(this Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.Amount,
        payment.Currency,
        payment.Status.ToString(),
        payment.FailureReason);
}
