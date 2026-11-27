using System.Linq.Expressions;
using Domain.Shared.Layer.Specifications;
using OrderFlow.Domain.Shared.Orders;

namespace OrderFlow.Domain.Orders.Specifications;

/// <summary>The orders of one customer ("my orders").</summary>
/// <param name="customerId">The customer.</param>
public sealed class OrdersOfCustomerSpecification(Guid customerId) : Specification<Order>
{
    /// <inheritdoc />
    public override Expression<Func<Order, bool>> ToExpression() => order => order.CustomerId == customerId;
}

/// <summary>The orders in one status (operations queues).</summary>
/// <param name="status">The status.</param>
public sealed class OrdersInStatusSpecification(OrderStatus status) : Specification<Order>
{
    /// <inheritdoc />
    public override Expression<Func<Order, bool>> ToExpression() => order => order.Status == status;
}

/// <summary>The order with a business number.</summary>
/// <param name="orderNumber">The order number.</param>
public sealed class OrderByNumberSpecification(string orderNumber) : Specification<Order>
{
    /// <inheritdoc />
    public override Expression<Func<Order, bool>> ToExpression() => order => order.OrderNumber == orderNumber;
}
