using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Repositories;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Payments.Specifications;
using OrderFlow.Domain.Shared.Payments;

namespace OrderFlow.Domain.Payments;

/// <summary>
/// Keeps one successful payment per order (a rule across payments). It does not save.
/// </summary>
/// <param name="paymentRepository">The payment repository.</param>
/// <param name="queryableExecuter">Executes the queries.</param>
public sealed class PaymentManager(IPaymentRepository paymentRepository, IAsyncQueryableExecuter queryableExecuter)
{
    /// <summary>Starts a payment for an order that has no completed payment.</summary>
    /// <param name="orderId">The order.</param>
    /// <param name="amount">The order total.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<Payment> StartAsync(Guid orderId, Money amount, CancellationToken cancellationToken = default)
    {
        await EnsureNoOtherCompletedPaymentAsync(orderId, exceptPaymentId: null, cancellationToken).ConfigureAwait(false);

        return Payment.Start(orderId, amount);
    }

    /// <summary>Records the successful attempt when no other payment of the order is completed.</summary>
    /// <param name="payment">The payment.</param>
    /// <param name="providerReference">The provider id of the payment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task CompleteAsync(Payment payment, string? providerReference, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        await EnsureNoOtherCompletedPaymentAsync(payment.OrderId, payment.Id, cancellationToken).ConfigureAwait(false);

        payment.RecordSuccessfulAttempt(providerReference);
    }

    private async Task EnsureNoOtherCompletedPaymentAsync(Guid orderId, Guid? exceptPaymentId, CancellationToken cancellationToken)
    {
        var query = paymentRepository.GetQueryable()
            .Where(new PaymentsOfOrderSpecification(orderId).And(new CompletedPaymentsSpecification()).ToExpression());

        if (exceptPaymentId is { } paymentId)
        {
            query = query.Where(payment => payment.Id != paymentId);
        }

        if (await queryableExecuter.CountAsync(query, cancellationToken).ConfigureAwait(false) > 0)
        {
            throw new ErrorException(ErrorCode.Conflict, PaymentsMessageKeys.OrderAlreadyPaid, orderId);
        }
    }
}
