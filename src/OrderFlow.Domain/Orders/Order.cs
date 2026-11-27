using Domain.Shared.Layer.Context;
using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Orders.Events;
using OrderFlow.Domain.Shared.Orders;

namespace OrderFlow.Domain.Orders;

/// <summary>
/// A customer's request to buy products, in one currency (glossary). The aggregate root of Orders.
/// Invariants: at least one item; every item in the order currency; quantity above zero; one item
/// per product; the total equals the sum of the items; only the lifecycle transitions of ADR-0001.
/// Items are given when the order is placed and never change afterwards.
/// </summary>
public sealed class Order : AggregateRoot
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
    {
        [OrderStatus.Placed] = [OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Confirmed] = [OrderStatus.Paid, OrderStatus.Cancelled],
        [OrderStatus.Paid] = [],
        [OrderStatus.Cancelled] = [],
    };

    private readonly List<OrderItem> _items = [];

    private Order(Guid id, string orderNumber, Guid customerId, string currency, Address deliveryAddress, RequestSource source)
        : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Currency = currency;
        DeliveryAddress = deliveryAddress;
        Source = source;
        Status = OrderStatus.Placed;
        Total = Money.Zero(currency);
    }

    /// <summary>The unique business number. Different from <see cref="Entity.Id"/>.</summary>
    public string OrderNumber { get; private set; }

    /// <summary>The customer who placed the order (a Customer id).</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>The one currency of the order (ADR-0003).</summary>
    public string Currency { get; private set; }

    /// <summary>The lifecycle state.</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>The delivery address.</summary>
    public Address DeliveryAddress { get; private set; }

    /// <summary>Where the request came from (ADR-0013).</summary>
    public RequestSource Source { get; private set; }

    /// <summary>The sum of the items, in <see cref="Currency"/>.</summary>
    public Money Total { get; private set; }

    /// <summary>The product lines.</summary>
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    /// <summary>Places a new order with its items. Raises <see cref="OrderPlaced"/>.</summary>
    /// <param name="orderNumber">The unique business number (from <see cref="IOrderNumberGenerator"/>).</param>
    /// <param name="customerId">The customer.</param>
    /// <param name="currency">The order currency.</param>
    /// <param name="deliveryAddress">The delivery address.</param>
    /// <param name="source">The request source.</param>
    /// <param name="lines">The items: at least one, one per product.</param>
    public static Order Place(
        string? orderNumber,
        Guid customerId,
        string? currency,
        Address deliveryAddress,
        RequestSource source,
        IReadOnlyCollection<OrderItemLine> lines)
    {
        ArgumentNullException.ThrowIfNull(deliveryAddress);
        ArgumentNullException.ThrowIfNull(lines);

        var number = Check.Text(orderNumber, nameof(OrderNumber), OrderConstants.OrderNumberMaxLength, OrdersMessageKeys.FieldRequired, OrdersMessageKeys.FieldTooLong);
        Check.NotEmpty(customerId, nameof(CustomerId), OrdersMessageKeys.FieldRequired);
        var code = Money.NormalizeCurrency(currency);

        if (lines.Count == 0)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, OrdersMessageKeys.OrderHasNoItems);
        }

        var order = new Order(Guid.CreateVersion7(), number, customerId, code, deliveryAddress, source);

        foreach (var line in lines)
        {
            order.AddItem(line);
        }

        order.AddDomainEvent(new OrderPlaced(order.Id, order.OrderNumber, order.CustomerId, order.Total));

        return order;
    }

    /// <summary>The stock of every product is reserved (ADR-0001). Raises <see cref="OrderConfirmed"/>.</summary>
    public void Confirm()
    {
        ChangeStatus(OrderStatus.Confirmed);
        AddDomainEvent(new OrderConfirmed(Id));
    }

    /// <summary>The payment completed. Raises <see cref="OrderPaid"/>.</summary>
    public void MarkPaid()
    {
        ChangeStatus(OrderStatus.Paid);
        AddDomainEvent(new OrderPaid(Id, Total));
    }

    /// <summary>Ends the order before payment (for example an expired reservation). Raises <see cref="OrderCancelled"/>.</summary>
    public void Cancel()
    {
        var previous = Status;
        ChangeStatus(OrderStatus.Cancelled);
        AddDomainEvent(new OrderCancelled(Id, previous));
    }

    /// <summary>Returns true when the lifecycle allows moving from the current status to the target.</summary>
    /// <param name="target">The requested status.</param>
    public bool CanChangeTo(OrderStatus target) =>
        AllowedTransitions.TryGetValue(Status, out var targets) && targets.Contains(target);

    private void ChangeStatus(OrderStatus target)
    {
        if (!CanChangeTo(target))
        {
            throw new ErrorException(ErrorCode.Conflict, OrdersMessageKeys.InvalidStatusTransition, Status, target);
        }

        Status = target;
    }

    private void AddItem(OrderItemLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        ArgumentNullException.ThrowIfNull(line.UnitPrice);

        Check.NotEmpty(line.ProductId, nameof(OrderItem.ProductId), OrdersMessageKeys.FieldRequired);

        if (line.Quantity <= 0)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, OrdersMessageKeys.QuantityNotPositive, line.ProductId, line.Quantity);
        }

        if (!string.Equals(line.UnitPrice.Currency, Currency, StringComparison.Ordinal))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, OrdersMessageKeys.ItemCurrencyMismatch, line.ProductId, line.UnitPrice.Currency, Currency);
        }

        if (_items.Exists(item => item.ProductId == line.ProductId))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, OrdersMessageKeys.DuplicateProduct, line.ProductId);
        }

        var item = new OrderItem(Guid.CreateVersion7(), line.ProductId, line.Quantity, line.UnitPrice);
        _items.Add(item);
        Total = Total.Add(item.LineTotal);
    }
}
