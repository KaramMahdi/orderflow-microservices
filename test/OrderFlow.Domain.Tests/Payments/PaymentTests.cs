using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Payments;
using OrderFlow.Domain.Payments.Events;
using OrderFlow.Domain.Shared.Payments;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Payments;

/// <summary>Payment, its attempts, the refund, the order total and one successful payment per order.</summary>
public sealed class PaymentTests
{
    private static readonly Money Amount = Money.Create(25m, "USD");

    [Fact]
    public void AFailedAttemptLeavesThePaymentOpen()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        payment.RecordFailedAttempt(null, "card declined");
        payment.RecordSuccessfulAttempt("PSP-1", Amount);

        Assert.Equal(PaymentStatus.Completed, payment.Status);
        Assert.Equal("PSP-1", payment.ProviderReference);
        Assert.Equal([PaymentAttemptResult.Failed, PaymentAttemptResult.Succeeded], payment.Attempts.Select(attempt => attempt.Result));
        Assert.Collection(
            payment.DomainEvents,
            domainEvent => Assert.IsType<PaymentFailed>(domainEvent),
            domainEvent => Assert.IsType<PaymentCompleted>(domainEvent));
    }

    [Fact]
    public void NoAttemptAfterCompletion()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);
        payment.RecordSuccessfulAttempt("PSP-1", Amount);

        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotOpen, () => payment.RecordSuccessfulAttempt("PSP-2", Amount));
        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotOpen, () => payment.RecordFailedAttempt(null, null));
    }

    [Fact]
    public void TheAmountIsAboveZero()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, PaymentsMessageKeys.AmountNotPositive, () => Payment.Start(Guid.CreateVersion7(), Money.Zero("USD")));
    }

    [Theory]
    [InlineData(24.99, "USD")]
    [InlineData(25.01, "USD")]
    [InlineData(25, "EUR")]
    public void TheCollectedAmountMustEqualTheOrderTotal(decimal amount, string currency)
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.CollectedAmountMismatch, () => payment.RecordSuccessfulAttempt("PSP-1", Money.Create(amount, currency)));

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Empty(payment.Attempts);
        Assert.Empty(payment.DomainEvents);
    }

    [Fact]
    public void RefundHappensOnceAndOnlyAfterCompletion()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotCompleted, payment.Refund);

        payment.RecordSuccessfulAttempt("PSP-1", Amount);
        payment.Refund();
        payment.Refund();

        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Single(payment.DomainEvents, domainEvent => domainEvent is PaymentRefunded);
    }

    [Fact]
    public async Task ManagerStartsThePaymentWithTheOrderTotal()
    {
        var orderId = Guid.CreateVersion7();
        var manager = Manager([], new() { [orderId] = Amount });

        var payment = await manager.StartAsync(orderId, TestContext.Current.CancellationToken);

        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(Amount, payment.Amount);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public async Task ManagerRejectsAnUnknownOrder()
    {
        var manager = Manager([], []);

        await ErrorAssert.ThrowsAsync(ErrorCode.NotFound, PaymentsMessageKeys.OrderNotFound, () => manager.StartAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ManagerKeepsOneSuccessfulPaymentPerOrder()
    {
        var orderId = Guid.CreateVersion7();
        var completed = Payment.Start(orderId, Amount);
        completed.RecordSuccessfulAttempt("PSP-1", Amount);
        var second = Payment.Start(orderId, Amount);
        var manager = Manager([completed, second], new() { [orderId] = Amount });
        var cancellationToken = TestContext.Current.CancellationToken;

        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, PaymentsMessageKeys.OrderAlreadyPaid, () => manager.StartAsync(orderId, cancellationToken));
        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, PaymentsMessageKeys.OrderAlreadyPaid, () => manager.CompleteAsync(second, "PSP-2", Amount, cancellationToken));
        Assert.Equal(PaymentStatus.Pending, second.Status);
    }

    [Fact]
    public async Task ManagerCompletesTheOnlyPaymentOfAnOrder()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);
        var manager = Manager([payment], []);

        await manager.CompleteAsync(payment, "PSP-1", Amount, TestContext.Current.CancellationToken);

        Assert.Equal(PaymentStatus.Completed, payment.Status);
    }

    private static PaymentManager Manager(List<Payment> payments, Dictionary<Guid, Money> totals) =>
        new(new PaymentRepository(payments), new InMemoryQueryableExecuter(), new OrderTotalReader(totals));

    private sealed class PaymentRepository(List<Payment> payments) : IPaymentRepository
    {
        public IQueryable<Payment> GetQueryable() => payments.AsQueryable();
    }

    private sealed class OrderTotalReader(Dictionary<Guid, Money> totals) : IOrderTotalReader
    {
        public Task<Money?> GetTotalAsync(Guid orderId, CancellationToken cancellationToken = default) =>
            Task.FromResult(totals.GetValueOrDefault(orderId));
    }
}
