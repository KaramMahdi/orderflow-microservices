using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Payments;
using OrderFlow.Domain.Payments.Events;
using OrderFlow.Domain.Shared.Payments;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Payments;

/// <summary>Payment, its attempts, the refund and one successful payment per order.</summary>
public sealed class PaymentTests
{
    private static readonly Money Amount = Money.Create(25m, "USD");

    [Fact]
    public void AFailedAttemptLeavesThePaymentOpen()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        payment.RecordFailedAttempt(null, "card declined");
        payment.RecordSuccessfulAttempt("PSP-1");

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
        payment.RecordSuccessfulAttempt("PSP-1");

        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotOpen, () => payment.RecordSuccessfulAttempt("PSP-2"));
        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotOpen, () => payment.RecordFailedAttempt(null, null));
    }

    [Fact]
    public void TheAmountIsAboveZero()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, PaymentsMessageKeys.AmountNotPositive, () => Payment.Start(Guid.CreateVersion7(), Money.Zero("USD")));
    }

    [Fact]
    public void RefundHappensOnceAndOnlyAfterCompletion()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotCompleted, payment.Refund);

        payment.RecordSuccessfulAttempt("PSP-1");
        payment.Refund();
        payment.Refund();

        Assert.Equal(PaymentStatus.Refunded, payment.Status);
        Assert.Single(payment.DomainEvents, domainEvent => domainEvent is PaymentRefunded);
    }

    [Fact]
    public async Task ManagerKeepsOneSuccessfulPaymentPerOrder()
    {
        var orderId = Guid.CreateVersion7();
        var completed = Payment.Start(orderId, Amount);
        completed.RecordSuccessfulAttempt("PSP-1");
        var second = Payment.Start(orderId, Amount);
        var manager = new PaymentManager(new PaymentRepository([completed, second]), new InMemoryQueryableExecuter());
        var cancellationToken = TestContext.Current.CancellationToken;

        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, PaymentsMessageKeys.OrderAlreadyPaid, () => manager.StartAsync(orderId, Amount, cancellationToken));
        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, PaymentsMessageKeys.OrderAlreadyPaid, () => manager.CompleteAsync(second, "PSP-2", cancellationToken));
        Assert.Equal(PaymentStatus.Pending, second.Status);
    }

    [Fact]
    public async Task ManagerCompletesTheOnlyPaymentOfAnOrder()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);
        var manager = new PaymentManager(new PaymentRepository([payment]), new InMemoryQueryableExecuter());

        await manager.CompleteAsync(payment, "PSP-1", TestContext.Current.CancellationToken);

        Assert.Equal(PaymentStatus.Completed, payment.Status);
    }

    private sealed class PaymentRepository(List<Payment> payments) : IPaymentRepository
    {
        public IQueryable<Payment> GetQueryable() => payments.AsQueryable();
    }
}
