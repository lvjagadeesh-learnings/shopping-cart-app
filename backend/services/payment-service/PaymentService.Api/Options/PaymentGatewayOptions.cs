namespace PaymentService.Api.Options;

public sealed class PaymentGatewayOptions
{
    public const string SectionName = "Payments";

    /// <summary>
    /// Fraction (0.0-1.0) of authorize calls that should simulate a declined payment.
    /// Defaults to 0 (always succeed). Override per-environment to exercise failure paths.
    /// </summary>
    public double SimulatedFailureRate { get; set; }
}
