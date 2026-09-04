namespace PaymentService.Api.Domain;

public enum TransactionType
{
    Authorization
}

/// <summary>
/// Append-only audit trail of every gateway attempt made against a <see cref="Payment"/>.
/// </summary>
public sealed class PaymentTransaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid PaymentId { get; init; }
    public required TransactionType Type { get; init; }
    public required bool Succeeded { get; init; }
    public string? FailureReason { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
