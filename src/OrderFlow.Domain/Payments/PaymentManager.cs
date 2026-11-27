using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Repositories;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Payments.Specifications;
using OrderFlow.Domain.Shared.Payments;

namespace OrderFlow.Domain.Payments;

/// <summary>
/// The only way to start a payment. Keeps the rules that span payments and orders: the amount is the
/// order total (read through <see cref="IOrderTotalReader"/>, never given by the caller), and one
/// successful payment per order. It does not save.
/// </summary>
/// <param name="paymentRepository">The payment repository.</param>
/// <param name="queryableExecuter">Executes the queries.</param>
/// <param name="orderTotalReader">Reads the order total.</param>
public sealed class PaymentManager(
    IPaymentRepository paymentRepository,
    IAsyncQueryableExecuter queryableExecuter,
    IOrderTotalReader orderTotalReader)
{
    /// <summary>Starts a payment of the order total for an order that has no completed payment.</summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<Payment> StartAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var total = await orderTotalReader.GetTotalAsync(orderId, cancellationToken).ConfigureAwait(false)
            ?? throw new ErrorException(ErrorCode.NotFound, PaymentsMessageKeys.OrderNotFound, orderId);

        await EnsureNoOtherCompletedPaymentAsync(orderId, exceptPaymentId: null, cancellationToken).ConfigureAwait(false);

        return Payment.Start(orderId, total);
    }

    /// <summary>Records the successful attempt when no other payment of the order is completed.</summary>
    /// <param name="payment">The payment.</param>
    /// <param name="providerReference">The provider id of the payment.</param>
    /// <param name="collectedAmount">The amount the provider collected.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task CompleteAsync(Payment payment, string? providerReference, Money collectedAmount, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payment);

        await EnsureNoOtherCompletedPaymentAsync(payment.OrderId, payment.Id, cancellationToken).ConfigureAwait(false);

        payment.RecordSuccessfulAttempt(providerReference, collectedAmount);
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
