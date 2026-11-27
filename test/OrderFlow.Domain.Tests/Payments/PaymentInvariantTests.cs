using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Payments;
using OrderFlow.Domain.Payments.Specifications;
using OrderFlow.Domain.Shared.Payments;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Payments;

/// <summary>The remaining Payment invariants.</summary>
public sealed class PaymentInvariantTests
{
    private static readonly Money Amount = Money.Create(10m, "USD");

    [Fact]
    public void APaymentNeedsAnOrder()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, PaymentsMessageKeys.FieldRequired, () => Payment.Start(Guid.Empty, Amount));
    }

    [Fact]
    public void ANewPaymentIsPendingWithoutAttempts()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(Amount, payment.Amount);
        Assert.Empty(payment.Attempts);
        Assert.Null(payment.ProviderReference);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void ASuccessfulAttemptNeedsAProviderReference(string? reference)
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        ErrorAssert.Throws(ErrorCode.ValidationFailed, PaymentsMessageKeys.FieldRequired, () => payment.RecordSuccessfulAttempt(reference));
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Empty(payment.Attempts);
    }

    [Fact]
    public void TheProviderReferenceHasAMaximumLength()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);
        var tooLong = new string('r', PaymentConstants.ProviderReferenceMaxLength + 1);

        ErrorAssert.Throws(ErrorCode.ValidationFailed, PaymentsMessageKeys.FieldTooLong, () => payment.RecordSuccessfulAttempt(tooLong));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, PaymentsMessageKeys.FieldTooLong, () => payment.RecordFailedAttempt(tooLong, null));
    }

    [Fact]
    public void SeveralFailedAttemptsAreAllowed()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        payment.RecordFailedAttempt("PSP-1", "declined");
        payment.RecordFailedAttempt(null, null);

        Assert.Equal(PaymentStatus.Failed, payment.Status);
        Assert.Equal(2, payment.Attempts.Count);
        Assert.Null(payment.Attempts[1].FailureReason);
    }

    [Fact]
    public void TheFailureReasonIsCut()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);

        payment.RecordFailedAttempt(null, new string('x', PaymentConstants.FailureReasonMaxLength + 5));

        Assert.Equal(PaymentConstants.FailureReasonMaxLength, payment.Attempts[0].FailureReason!.Length);
    }

    [Fact]
    public void AFailedPaymentCannotBeRefunded()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);
        payment.RecordFailedAttempt(null, null);

        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotCompleted, payment.Refund);
    }

    [Fact]
    public void ARefundedPaymentAcceptsNoAttempt()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);
        payment.RecordSuccessfulAttempt("PSP-1");
        payment.Refund();

        ErrorAssert.Throws(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotOpen, () => payment.RecordSuccessfulAttempt("PSP-2"));
    }

    [Fact]
    public async Task ARefundedPaymentDoesNotBlockANewPayment()
    {
        var orderId = Guid.CreateVersion7();
        var refunded = Payment.Start(orderId, Amount);
        refunded.RecordSuccessfulAttempt("PSP-1");
        refunded.Refund();
        var manager = new PaymentManager(new PaymentRepository([refunded]), new InMemoryQueryableExecuter());

        var payment = await manager.StartAsync(orderId, Amount, TestContext.Current.CancellationToken);

        Assert.Equal(PaymentStatus.Pending, payment.Status);
    }

    [Fact]
    public void SpecificationsSelectByOrderStatusAndReference()
    {
        var payment = Payment.Start(Guid.CreateVersion7(), Amount);
        payment.RecordSuccessfulAttempt("PSP-1");

        Assert.True(new PaymentsOfOrderSpecification(payment.OrderId).And(new CompletedPaymentsSpecification()).IsSatisfiedBy(payment));
        Assert.True(new PaymentByProviderReferenceSpecification("PSP-1").IsSatisfiedBy(payment));
    }

    private sealed class PaymentRepository(List<Payment> payments) : IPaymentRepository
    {
        public IQueryable<Payment> GetQueryable() => payments.AsQueryable();
    }
}
