namespace PaymentService.Api.Domain;

public enum PaymentStatus
{
    Pending,
    Authorized,
    Failed
}

public sealed class Payment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrderId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
