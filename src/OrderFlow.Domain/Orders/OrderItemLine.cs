using Domain.Shared.Layer.ValueObjects;

namespace OrderFlow.Domain.Orders;

/// <summary>The input of one item when an order is placed. <see cref="Order.Place"/> validates it.</summary>
/// <param name="ProductId">The product.</param>
/// <param name="Quantity">The quantity.</param>
/// <param name="UnitPrice">The price of one unit in the order currency.</param>
public sealed record OrderItemLine(Guid ProductId, int Quantity, Money UnitPrice);
