using Domain.Shared.Layer.Context;
using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Orders;
using OrderFlow.Domain.Orders.Events;
using OrderFlow.Domain.Shared.Orders;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Orders;

/// <summary>The Order aggregate: one currency, items, total and the ADR-0001 lifecycle.</summary>
public sealed class OrderTests
{
    private const string Usd = "USD";
    private const string OrderNumber = "ORD-1";

    private static readonly Address DeliveryAddress = Address.Create("1 Main St", "Amman", null, "JO");

    public static TheoryData<OrderStatus, OrderStatus> AllowedTransitions => new()
    {
        { OrderStatus.Placed, OrderStatus.Confirmed },
        { OrderStatus.Placed, OrderStatus.Cancelled },
        { OrderStatus.Confirmed, OrderStatus.Paid },
        { OrderStatus.Confirmed, OrderStatus.Cancelled },
    };

    [Fact]
    public void PlaceSumsTheItemsAndRaisesOrderPlaced()
    {
        var order = Place(Line(2, 10.25m), Line(1, 5m));

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal(Money.Create(25.5m, Usd), order.Total);
        Assert.Equal(2, order.Items.Count);
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(7, order.Id.Version);
        var placed = Assert.IsType<OrderPlaced>(Assert.Single(order.DomainEvents));
        Assert.Equal(order.Id, placed.OrderId);
        Assert.Equal(order.Total, placed.Total);
    }

    [Fact]
    public void PlaceNeedsAtLeastOneItem()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.OrderHasNoItems, () => Place());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void QuantityMustBeAboveZero(int quantity)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.QuantityNotPositive, () => Place(Line(quantity, 1m)));
    }

    [Fact]
    public void EveryItemIsInTheOrderCurrency()
    {
        var line = new OrderItemLine(Guid.CreateVersion7(), 1, Money.Create(1m, "EUR"));

        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.ItemCurrencyMismatch, () => Place(line));
    }

    [Fact]
    public void AProductAppearsOnce()
    {
        var line = Line(1, 1m);

        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.DuplicateProduct, () => Place(line, line with { Quantity = 2 }));
    }

    [Fact]
    public void PlaceRequiresAnOrderNumberAndACustomer()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.FieldRequired, () => Order.Place(" ", Guid.CreateVersion7(), Usd, DeliveryAddress, RequestSource.Web, [Line(1, 1m)]));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.FieldRequired, () => Order.Place(OrderNumber, Guid.Empty, Usd, DeliveryAddress, RequestSource.Web, [Line(1, 1m)]));
    }

    [Theory]
    [MemberData(nameof(AllowedTransitions))]
    public void AllowedTransitionsSucceed(OrderStatus from, OrderStatus to)
    {
        var order = InStatus(from);

        Move(order, to);

        Assert.Equal(to, order.Status);
    }

    [Theory]
    [InlineData(OrderStatus.Placed, OrderStatus.Paid)]
    [InlineData(OrderStatus.Paid, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Paid, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Confirmed)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Paid)]
    [InlineData(OrderStatus.Cancelled, OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Confirmed, OrderStatus.Confirmed)]
    public void OtherTransitionsAreRejected(OrderStatus from, OrderStatus to)
    {
        var order = InStatus(from);

        ErrorAssert.Throws(ErrorCode.Conflict, OrdersMessageKeys.InvalidStatusTransition, () => Move(order, to));
        Assert.Equal(from, order.Status);
    }

    [Fact]
    public void EachTransitionRaisesItsEvent()
    {
        var order = Place(Line(1, 1m));

        order.Confirm();
        order.MarkPaid();

        Assert.Collection(
            order.DomainEvents,
            domainEvent => Assert.IsType<OrderPlaced>(domainEvent),
            domainEvent => Assert.IsType<OrderConfirmed>(domainEvent),
            domainEvent => Assert.IsType<OrderPaid>(domainEvent));
    }

    [Fact]
    public void CancelRecordsThePreviousStatus()
    {
        var order = InStatus(OrderStatus.Confirmed);

        order.Cancel();

        var cancelled = Assert.IsType<OrderCancelled>(order.DomainEvents[^1]);
        Assert.Equal(OrderStatus.Confirmed, cancelled.PreviousStatus);
    }

    [Fact]
    public async Task ManagerUsesTheGeneratedNumber()
    {
        var manager = new OrderManager(new FixedNumberGenerator(OrderNumber));

        var order = await manager.PlaceAsync(Guid.CreateVersion7(), Usd, DeliveryAddress, RequestSource.Mobile, [Line(1, 1m)], TestContext.Current.CancellationToken);

        Assert.Equal(OrderNumber, order.OrderNumber);
        Assert.Equal(RequestSource.Mobile, order.Source);
    }

    private static OrderItemLine Line(int quantity, decimal unitPrice) =>
        new(Guid.CreateVersion7(), quantity, Money.Create(unitPrice, Usd));

    private static Order Place(params OrderItemLine[] lines) =>
        Order.Place(OrderNumber, Guid.CreateVersion7(), Usd, DeliveryAddress, RequestSource.Web, lines);

    private static Order InStatus(OrderStatus status)
    {
        var order = Place(Line(1, 1m));

        switch (status)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Paid:
                order.Confirm();
                order.MarkPaid();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
        }

        return order;
    }

    private static void Move(Order order, OrderStatus to)
    {
        switch (to)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                break;
            case OrderStatus.Paid:
                order.MarkPaid();
                break;
            case OrderStatus.Cancelled:
                order.Cancel();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(to));
        }
    }

    private sealed class FixedNumberGenerator(string number) : IOrderNumberGenerator
    {
        public Task<string> GenerateAsync(CancellationToken cancellationToken = default) => Task.FromResult(number);
    }
}
