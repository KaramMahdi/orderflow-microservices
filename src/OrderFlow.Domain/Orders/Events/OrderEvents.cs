using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Shared.Orders;

namespace OrderFlow.Domain.Orders.Events;

/// <summary>An order was placed.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="OrderNumber">The business number.</param>
/// <param name="CustomerId">The customer.</param>
/// <param name="Total">The order total.</param>
public sealed record OrderPlaced(Guid OrderId, string OrderNumber, Guid CustomerId, Money Total) : IDomainEvent;

/// <summary>The stock of every product of the order is reserved.</summary>
/// <param name="OrderId">The order.</param>
public sealed record OrderConfirmed(Guid OrderId) : IDomainEvent;

/// <summary>The order was paid.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="Total">The paid total.</param>
public sealed record OrderPaid(Guid OrderId, Money Total) : IDomainEvent;

/// <summary>The order was cancelled.</summary>
/// <param name="OrderId">The order.</param>
/// <param name="PreviousStatus">The status before the cancellation.</param>
public sealed record OrderCancelled(Guid OrderId, OrderStatus PreviousStatus) : IDomainEvent;
