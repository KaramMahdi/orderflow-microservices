using System.Linq.Expressions;
using Domain.Shared.Layer.Specifications;
using OrderFlow.Domain.Shared.Payments;

namespace OrderFlow.Domain.Payments.Specifications;

/// <summary>The payments of an order.</summary>
/// <param name="orderId">The order.</param>
public sealed class PaymentsOfOrderSpecification(Guid orderId) : Specification<Payment>
{
    /// <inheritdoc />
    public override Expression<Func<Payment, bool>> ToExpression() => payment => payment.OrderId == orderId;
}

/// <summary>The completed payments.</summary>
public sealed class CompletedPaymentsSpecification : Specification<Payment>
{
    /// <inheritdoc />
    public override Expression<Func<Payment, bool>> ToExpression() => payment => payment.Status == PaymentStatus.Completed;
}

/// <summary>The payment with a provider reference (provider callbacks).</summary>
/// <param name="providerReference">The provider reference.</param>
public sealed class PaymentByProviderReferenceSpecification(string providerReference) : Specification<Payment>
{
    /// <inheritdoc />
    public override Expression<Func<Payment, bool>> ToExpression() => payment => payment.ProviderReference == providerReference;
}
