using Domain.Shared.Layer.Context;
using Domain.Shared.Layer.ValueObjects;

namespace OrderFlow.Domain.Orders;

/// <summary>
/// Places orders with a unique order number, which needs <see cref="IOrderNumberGenerator"/>.
/// It does not save; the Application Service saves through the unit of work.
/// </summary>
/// <param name="orderNumberGenerator">The order number source.</param>
public sealed class OrderManager(IOrderNumberGenerator orderNumberGenerator)
{
    /// <summary>Places a new order. See <see cref="Order.Place"/>.</summary>
    /// <param name="customerId">The customer.</param>
    /// <param name="currency">The order currency.</param>
    /// <param name="deliveryAddress">The delivery address.</param>
    /// <param name="source">The request source.</param>
    /// <param name="lines">The items.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<Order> PlaceAsync(
        Guid customerId,
        string? currency,
        Address deliveryAddress,
        RequestSource source,
        IReadOnlyCollection<OrderItemLine> lines,
        CancellationToken cancellationToken = default)
    {
        var orderNumber = await orderNumberGenerator.GenerateAsync(cancellationToken).ConfigureAwait(false);

        return Order.Place(orderNumber, customerId, currency, deliveryAddress, source, lines);
    }
}
