using BuildingBlocks.Auth;
using PaymentService.Api.Contracts;
using PaymentService.Api.Services;

namespace PaymentService.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments").WithTags("Payments");

        // Internal service-to-service endpoint called by Order Service during checkout orchestration.
        group.MapPost("/authorize", async (AuthorizePaymentRequest request, PaymentManager manager, CancellationToken ct) =>
            Results.Ok(await manager.AuthorizeAsync(request, ct)));

        // Admin-only payment visibility (support/debugging).
        group.MapGet("/orders/{orderId:guid}", async (Guid orderId, PaymentManager manager, CancellationToken ct) =>
                Results.Ok(await manager.GetByOrderIdAsync(orderId, ct)))
            .RequireAuthorization(Policies.Admin);
    }
}
