using BuildingBlocks.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PaymentService.Api.Contracts;
using PaymentService.Api.Data;
using PaymentService.Api.Options;
using PaymentService.Api.Services;

namespace PaymentService.Tests;

public class PaymentManagerTests
{
    private static PaymentManager CreateSut(out PaymentDbContext db, double simulatedFailureRate = 0.0)
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        db = new PaymentDbContext(options);
        return new PaymentManager(db, Options.Create(new PaymentGatewayOptions { SimulatedFailureRate = simulatedFailureRate }));
    }

    [Fact]
    public async Task AuthorizeAsync_ValidRequest_DefaultRate_Succeeds()
    {
        var sut = CreateSut(out _);
        var orderId = Guid.NewGuid();

        var result = await sut.AuthorizeAsync(new AuthorizePaymentRequest(orderId, 49.99m, "usd"), CancellationToken.None);

        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(49.99m, result.Amount);
        Assert.Equal("USD", result.Currency);
        Assert.Equal("Authorized", result.Status);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public async Task AuthorizeAsync_ForcedFailureRate_ReturnsFailedStatusWithReason()
    {
        var sut = CreateSut(out _, simulatedFailureRate: 1.0);

        var result = await sut.AuthorizeAsync(new AuthorizePaymentRequest(Guid.NewGuid(), 20m, null), CancellationToken.None);

        Assert.Equal("Failed", result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.FailureReason));
    }

    [Fact]
    public async Task AuthorizeAsync_NullCurrency_DefaultsToUsd()
    {
        var sut = CreateSut(out _);

        var result = await sut.AuthorizeAsync(new AuthorizePaymentRequest(Guid.NewGuid(), 20m, null), CancellationToken.None);

        Assert.Equal("USD", result.Currency);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task AuthorizeAsync_NonPositiveAmount_ThrowsValidationApiException(decimal amount)
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<ValidationApiException>(
            () => sut.AuthorizeAsync(new AuthorizePaymentRequest(Guid.NewGuid(), amount, null), CancellationToken.None));
    }

    [Fact]
    public async Task AuthorizeAsync_InvalidCurrencyLength_ThrowsValidationApiException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<ValidationApiException>(
            () => sut.AuthorizeAsync(new AuthorizePaymentRequest(Guid.NewGuid(), 20m, "DOLLARS"), CancellationToken.None));
    }

    [Fact]
    public async Task AuthorizeAsync_DuplicateOrderId_IsIdempotent()
    {
        var sut = CreateSut(out var db, simulatedFailureRate: 1.0);
        var orderId = Guid.NewGuid();

        var first = await sut.AuthorizeAsync(new AuthorizePaymentRequest(orderId, 30m, null), CancellationToken.None);
        var second = await sut.AuthorizeAsync(new AuthorizePaymentRequest(orderId, 30m, null), CancellationToken.None);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(first.Status, second.Status);
        Assert.Equal(1, await db.Payments.CountAsync());
        Assert.Equal(1, await db.PaymentTransactions.CountAsync());
    }

    [Fact]
    public async Task AuthorizeAsync_PersistsPaymentTransactionRecord()
    {
        var sut = CreateSut(out var db);
        var orderId = Guid.NewGuid();

        var result = await sut.AuthorizeAsync(new AuthorizePaymentRequest(orderId, 15m, null), CancellationToken.None);

        var transaction = await db.PaymentTransactions.SingleAsync();
        Assert.Equal(result.Id, transaction.PaymentId);
        Assert.True(transaction.Succeeded);
        Assert.Null(transaction.FailureReason);
    }

    [Fact]
    public async Task GetByOrderIdAsync_ExistingPayment_ReturnsResponse()
    {
        var sut = CreateSut(out _);
        var orderId = Guid.NewGuid();
        await sut.AuthorizeAsync(new AuthorizePaymentRequest(orderId, 10m, null), CancellationToken.None);

        var result = await sut.GetByOrderIdAsync(orderId, CancellationToken.None);

        Assert.Equal(orderId, result.OrderId);
    }

    [Fact]
    public async Task GetByOrderIdAsync_UnknownOrder_ThrowsNotFoundException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.GetByOrderIdAsync(Guid.NewGuid(), CancellationToken.None));
    }
}
