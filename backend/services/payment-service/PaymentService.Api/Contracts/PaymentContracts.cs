namespace PaymentService.Api.Contracts;

public sealed record AuthorizePaymentRequest(Guid OrderId, decimal Amount, string? Currency);

public sealed record PaymentResponse(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Status,
    string? FailureReason);
