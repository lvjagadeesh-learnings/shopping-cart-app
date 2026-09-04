using BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentService.Api.Contracts;
using PaymentService.Api.Data;
using PaymentService.Api.Domain;
using PaymentService.Api.Options;

namespace PaymentService.Api.Services;

public sealed class PaymentManager(PaymentDbContext db, IOptions<PaymentGatewayOptions> options)
{
    /// <summary>
    /// Authorizes a simulated payment for an order. Idempotent: if a payment already exists for
    /// this order (e.g. a retried checkout request), it's returned as-is instead of re-authorizing.
    /// Outcome is randomized per <see cref="PaymentGatewayOptions.SimulatedFailureRate"/> so callers
    /// can exercise both the success and failure paths without a real payment gateway.
    /// </summary>
    public async Task<PaymentResponse> AuthorizeAsync(AuthorizePaymentRequest request, CancellationToken ct)
    {
        if (request.Amount <= 0)
        {
            throw new ValidationApiException("Amount must be greater than zero.");
        }

        var currency = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.Trim().ToUpperInvariant();
        if (currency.Length != 3)
        {
            throw new ValidationApiException("Currency must be a 3-letter ISO code.");
        }

        var existing = await db.Payments.FirstOrDefaultAsync(p => p.OrderId == request.OrderId, ct);
        if (existing is not null)
        {
            return existing.ToResponse();
        }

        var succeeded = Random.Shared.NextDouble() >= options.Value.SimulatedFailureRate;

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = currency,
            Status = succeeded ? PaymentStatus.Authorized : PaymentStatus.Failed,
            FailureReason = succeeded ? null : "Simulated gateway decline."
        };
        db.Payments.Add(payment);

        db.PaymentTransactions.Add(new PaymentTransaction
        {
            PaymentId = payment.Id,
            Type = TransactionType.Authorization,
            Succeeded = succeeded,
            FailureReason = payment.FailureReason
        });

        await db.SaveChangesAsync(ct);
        return payment.ToResponse();
    }

    public async Task<PaymentResponse> GetByOrderIdAsync(Guid orderId, CancellationToken ct)
    {
        var payment = await db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, ct)
            ?? throw new NotFoundException($"No payment found for order '{orderId}'.");

        return payment.ToResponse();
    }
}
