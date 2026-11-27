using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Catalog;
using OrderFlow.Domain.Catalog.Events;
using OrderFlow.Domain.Shared.Catalog;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Catalog;

/// <summary>The remaining Catalog invariants: Sku, Product name, prices and stock.</summary>
public sealed class CatalogInvariantTests
{
    private static readonly Guid ProductId = Guid.CreateVersion7();

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void SkuIsRequired(string? value)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.FieldRequired, () => Sku.Create(value));
    }

    [Fact]
    public void SkuHasAMaximumLength()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.FieldTooLong, () => Sku.Create(new string('A', CatalogConstants.SkuMaxLength + 1)));
    }

    [Fact]
    public void SkuComparesByValue()
    {
        Assert.Equal(Sku.Create("abc"), Sku.Create("ABC"));
    }

    [Fact]
    public void ProductNameIsRequired()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.FieldRequired, () => Product.Create(Sku.Create("P-1"), " "));
    }

    [Fact]
    public void ProductNameHasAMaximumLength()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.FieldTooLong, () => Product.Create(Sku.Create("P-1"), new string('n', CatalogConstants.ProductNameMaxLength + 1)));
    }

    [Fact]
    public void AnInvalidRenameKeepsTheOldName()
    {
        var product = Product.Create(Sku.Create("P-1"), "أحمد");

        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.FieldRequired, () => product.Rename(null));

        Assert.Equal("أحمد", product.Name);
        Assert.Equal("احمد", product.NormalizedName);
    }

    [Fact]
    public void ANewProductHasANormalizedNameAndNoPrices()
    {
        var product = Product.Create(Sku.Create("P-1"), " مدرسة ");

        Assert.Equal("مدرسة", product.Name);
        Assert.Equal("مدرسه", product.NormalizedName);
        Assert.Empty(product.Prices);
        Assert.Empty(product.DomainEvents);
    }

    [Fact]
    public void APriceIsNeverNegative()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountNegative, () => Product.Create(Sku.Create("P-1"), "Tea").SetPrice(Money.Create(-1m, "USD")));
    }

    [Fact]
    public void GetPriceRejectsAnInvalidCurrency()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.CurrencyCodeInvalid, () => Product.Create(Sku.Create("P-1"), "Tea").GetPrice("X"));
    }

    [Fact]
    public void StockNeedsAProduct()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.FieldRequired, () => StockItem.Create(Guid.Empty, 1));
    }

    [Fact]
    public void AReservationNeedsAnOrder()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.FieldRequired, () => StockItem.Create(ProductId, 1).Reserve(Guid.Empty, 1));
    }

    [Fact]
    public void CompletingOrReleasingAnUnknownReservationIsNotFound()
    {
        var stock = StockItem.Create(ProductId, 1);

        ErrorAssert.Throws(ErrorCode.NotFound, CatalogMessageKeys.ReservationNotFound, () => stock.CompleteReservation(Guid.CreateVersion7()));
        ErrorAssert.Throws(ErrorCode.NotFound, CatalogMessageKeys.ReservationNotFound, () => stock.ReleaseReservation(Guid.CreateVersion7()));
    }

    [Fact]
    public void ReservingTheWholeAvailableQuantityIsAllowed()
    {
        var stock = StockItem.Create(ProductId, 3);
        var orderId = Guid.CreateVersion7();

        stock.Reserve(orderId, 3);

        Assert.Equal(stock.OnHand, stock.Reserved);
        Assert.Equal(0, stock.Available);
        Assert.True(stock.HasActiveReservation(orderId));
    }

    [Fact]
    public void ReservedNeverGoesAboveOnHandAcrossOrders()
    {
        var stock = StockItem.Create(ProductId, 5);
        stock.Reserve(Guid.CreateVersion7(), 2);
        stock.Reserve(Guid.CreateVersion7(), 3);

        var exception = ErrorAssert.Throws(ErrorCode.Conflict, CatalogMessageKeys.InsufficientStock, () => stock.Reserve(Guid.CreateVersion7(), 1));

        Assert.Equal([ProductId, 0, 1], exception.Args);
        Assert.True(stock.Reserved <= stock.OnHand);
        Assert.Equal(2, stock.Movements.Count);
    }

    [Fact]
    public void AFailedReservationDoesNotChangeTheCounters()
    {
        var stock = StockItem.Create(ProductId, 2);

        ErrorAssert.Throws(ErrorCode.Conflict, CatalogMessageKeys.InsufficientStock, () => stock.Reserve(Guid.CreateVersion7(), 3));

        Assert.Equal(2, stock.OnHand);
        Assert.Equal(0, stock.Reserved);
        Assert.Empty(stock.Movements);
        Assert.Empty(stock.DomainEvents);
    }

    [Fact]
    public void AReleasedQuantityCanBeReservedAgain()
    {
        var stock = StockItem.Create(ProductId, 1);
        var expired = Guid.CreateVersion7();
        stock.Reserve(expired, 1);
        stock.ReleaseReservation(expired);

        stock.Reserve(Guid.CreateVersion7(), 1);

        Assert.Equal(1, stock.Reserved);
    }

    [Fact]
    public void AdjustOnHandChangesTheAvailableQuantity()
    {
        var stock = StockItem.Create(ProductId, 0);

        stock.AdjustOnHand(7);

        Assert.Equal(7, stock.Available);
    }

    [Fact]
    public async Task ManagerRejectsAProductWithoutStock()
    {
        var manager = new StockManager(new StockRepository([]), new InMemoryQueryableExecuter());
        var quantities = new Dictionary<Guid, int> { [ProductId] = 1 };

        await ErrorAssert.ThrowsAsync(ErrorCode.NotFound, CatalogMessageKeys.StockItemNotFound, () => manager.ReserveForOrderAsync(Guid.CreateVersion7(), quantities, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ManagerReservesInProductIdOrder()
    {
        var first = StockItem.Create(Guid.CreateVersion7(), 5);
        var second = StockItem.Create(Guid.CreateVersion7(), 5);
        var manager = new StockManager(new StockRepository([second, first]), new InMemoryQueryableExecuter());
        var quantities = new Dictionary<Guid, int> { [second.ProductId] = 1, [first.ProductId] = 1 };

        var reserved = await manager.ReserveForOrderAsync(Guid.CreateVersion7(), quantities, TestContext.Current.CancellationToken);

        Assert.Equal(quantities.Keys.Order(), reserved.Select(stock => stock.ProductId));
    }

    [Fact]
    public async Task ManagerReleasesEveryReservationOfAnOrder()
    {
        var first = StockItem.Create(Guid.CreateVersion7(), 5);
        var second = StockItem.Create(Guid.CreateVersion7(), 5);
        var manager = new StockManager(new StockRepository([first, second]), new InMemoryQueryableExecuter());
        var orderId = Guid.CreateVersion7();
        var cancellationToken = TestContext.Current.CancellationToken;
        await manager.ReserveForOrderAsync(orderId, new Dictionary<Guid, int> { [first.ProductId] = 2, [second.ProductId] = 3 }, cancellationToken);

        await manager.ReleaseForOrderAsync(orderId, cancellationToken);

        Assert.Equal(0, first.Reserved);
        Assert.Equal(0, second.Reserved);
        Assert.Single(first.DomainEvents, domainEvent => domainEvent is StockReleased);
    }

    [Fact]
    public async Task ManagerCreatesStockForANewProduct()
    {
        var manager = new StockManager(new StockRepository([]), new InMemoryQueryableExecuter());

        var stock = await manager.CreateAsync(ProductId, 4, TestContext.Current.CancellationToken);

        Assert.Equal(ProductId, stock.ProductId);
        Assert.Equal(4, stock.OnHand);
    }

    private sealed class StockRepository(List<StockItem> stockItems) : IStockItemRepository
    {
        public IQueryable<StockItem> GetQueryable() => stockItems.AsQueryable();
    }
}
