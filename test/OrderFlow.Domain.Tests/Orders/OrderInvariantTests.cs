using System.Reflection;
using Domain.Shared.Layer.Context;
using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Orders;
using OrderFlow.Domain.Orders.Specifications;
using OrderFlow.Domain.Shared.Orders;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Orders;

/// <summary>The remaining Order invariants, the one-currency rule and "no items after payment".</summary>
public sealed class OrderInvariantTests
{
    private const string Usd = "USD";
    private const string OrderNumber = "ORD-1";

    private static readonly Address DeliveryAddress = Address.Create("1 Main St", "Amman", null, "JO");

    [Fact]
    public void TheOrderNumberHasAMaximumLength()
    {
        var tooLong = new string('9', OrderConstants.OrderNumberMaxLength + 1);

        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.FieldTooLong, () => Order.Place(tooLong, Guid.CreateVersion7(), Usd, DeliveryAddress, RequestSource.Web, [Line(Usd)]));
    }

    [Fact]
    public void AnItemNeedsAProduct()
    {
        var line = new OrderItemLine(Guid.Empty, 1, Money.Create(1m, Usd));

        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.FieldRequired, () => Place(Usd, line));
    }

    [Fact]
    public void TheOrderCurrencyMustBeValid()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.CurrencyCodeInvalid, () => Place("DOLLAR", Line(Usd)));
    }

    [Fact]
    public void TheOrderCurrencyIsNormalized()
    {
        var order = Place("usd", Line(Usd));

        Assert.Equal(Usd, order.Currency);
        Assert.Equal(Usd, order.Total.Currency);
    }

    [Fact]
    public void OneCurrencyEvenWhenOnlyTheLastItemDiffers()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, OrdersMessageKeys.ItemCurrencyMismatch, () => Place(Usd, Line(Usd), Line(Usd), Line("EUR")));
    }

    [Fact]
    public void EveryItemAndTheTotalShareTheOrderCurrency()
    {
        var order = Place(Usd, Line(Usd), Line(Usd));

        Assert.All(order.Items, item => Assert.Equal(order.Currency, item.UnitPrice.Currency));
        Assert.Equal(order.Currency, order.Total.Currency);
    }

    [Fact]
    public void TheTotalEqualsTheSumOfTheLineTotals()
    {
        var order = Place(
            Usd,
            new OrderItemLine(Guid.CreateVersion7(), 3, Money.Create(0.1m, Usd)),
            new OrderItemLine(Guid.CreateVersion7(), 7, Money.Create(19.99m, Usd)),
            new OrderItemLine(Guid.CreateVersion7(), 1, Money.Create(0m, Usd)));

        Assert.Equal(order.Items.Sum(item => item.LineTotal.Amount), order.Total.Amount);
        Assert.Equal(140.23m, order.Total.Amount);
    }

    [Fact]
    public void TheItemsCannotBeChangedFromOutside()
    {
        var order = Place(Usd, Line(Usd));
        var items = (IList<OrderItem>)order.Items;

        Assert.Throws<NotSupportedException>(() => items.Add(items[0]));
        Assert.Throws<NotSupportedException>(items.Clear);
    }

    [Fact]
    public void NoPublicMethodAddsOrRemovesItems()
    {
        var methods = typeof(Order)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal);

        Assert.Equal([nameof(Order.CanChangeTo), nameof(Order.Cancel), nameof(Order.Confirm), nameof(Order.MarkPaid)], methods);
    }

    [Fact]
    public void APaidOrderKeepsItsItemsAndTotal()
    {
        var order = Place(Usd, Line(Usd));
        var total = order.Total;
        order.Confirm();
        order.MarkPaid();

        Assert.Single(order.Items);
        Assert.Equal(total, order.Total);
        ErrorAssert.Throws(ErrorCode.Conflict, OrdersMessageKeys.InvalidStatusTransition, order.Cancel);
    }

    [Theory]
    [InlineData(OrderStatus.Confirmed, true)]
    [InlineData(OrderStatus.Cancelled, true)]
    [InlineData(OrderStatus.Paid, false)]
    [InlineData(OrderStatus.Placed, false)]
    public void CanChangeToFollowsTheLifecycle(OrderStatus target, bool expected)
    {
        Assert.Equal(expected, Place(Usd, Line(Usd)).CanChangeTo(target));
    }

    [Fact]
    public void TheRejectedTransitionNamesBothStatuses()
    {
        var order = Place(Usd, Line(Usd));

        var exception = ErrorAssert.Throws(ErrorCode.Conflict, OrdersMessageKeys.InvalidStatusTransition, order.MarkPaid);

        Assert.Equal([OrderStatus.Placed, OrderStatus.Paid], exception.Args);
    }

    [Fact]
    public void SpecificationsSelectByCustomerStatusAndNumber()
    {
        var order = Place(Usd, Line(Usd));

        Assert.True(new OrdersOfCustomerSpecification(order.CustomerId).IsSatisfiedBy(order));
        Assert.False(new OrdersOfCustomerSpecification(Guid.CreateVersion7()).IsSatisfiedBy(order));
        Assert.True(new OrdersInStatusSpecification(OrderStatus.Placed).IsSatisfiedBy(order));
        Assert.True(new OrderByNumberSpecification(OrderNumber).IsSatisfiedBy(order));
    }

    private static OrderItemLine Line(string currency) =>
        new(Guid.CreateVersion7(), 1, Money.Create(1m, currency));

    private static Order Place(string currency, params OrderItemLine[] lines) =>
        Order.Place(OrderNumber, Guid.CreateVersion7(), currency, DeliveryAddress, RequestSource.Web, lines);
}
