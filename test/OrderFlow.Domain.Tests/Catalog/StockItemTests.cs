using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Catalog;
using OrderFlow.Domain.Catalog.Events;
using OrderFlow.Domain.Shared.Catalog;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.Catalog;

/// <summary>StockItem and its movements (ADR-0001).</summary>
public sealed class StockItemTests
{
    private static readonly Guid ProductId = Guid.CreateVersion7();

    [Fact]
    public void ReserveHoldsTheQuantityAndRaisesStockReserved()
    {
        var stock = StockItem.Create(ProductId, 10);
        var orderId = Guid.CreateVersion7();

        var movement = stock.Reserve(orderId, 4);

        Assert.Equal(StockMovementStatus.Reserved, movement.Status);
        Assert.Equal(4, stock.Reserved);
        Assert.Equal(6, stock.Available);
        Assert.Equal(new StockReserved(stock.Id, ProductId, orderId, 4), Assert.Single(stock.DomainEvents));
    }

    [Fact]
    public void ReserveNeverGoesAboveOnHand()
    {
        var stock = StockItem.Create(ProductId, 1);
        stock.Reserve(Guid.CreateVersion7(), 1);

        ErrorAssert.Throws(ErrorCode.Conflict, CatalogMessageKeys.InsufficientStock, () => stock.Reserve(Guid.CreateVersion7(), 1));
        Assert.Equal(1, stock.Reserved);
    }

    [Fact]
    public void OneReservationPerOrder()
    {
        var stock = StockItem.Create(ProductId, 10);
        var orderId = Guid.CreateVersion7();
        stock.Reserve(orderId, 1);

        ErrorAssert.Throws(ErrorCode.Conflict, CatalogMessageKeys.ReservationAlreadyExists, () => stock.Reserve(orderId, 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void ReservedQuantityIsAboveZero(int quantity)
    {
        var stock = StockItem.Create(ProductId, 10);

        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.QuantityNotPositive, () => stock.Reserve(Guid.CreateVersion7(), quantity));
    }

    [Fact]
    public void QuantitiesAreNeverNegative()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.QuantityNegative, () => StockItem.Create(ProductId, -1));
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CatalogMessageKeys.QuantityNegative, () => StockItem.Create(ProductId, 0).AdjustOnHand(-1));
    }

    [Fact]
    public void OnHandNeverGoesBelowReserved()
    {
        var stock = StockItem.Create(ProductId, 5);
        stock.Reserve(Guid.CreateVersion7(), 3);

        ErrorAssert.Throws(ErrorCode.Conflict, CatalogMessageKeys.OnHandBelowReserved, () => stock.AdjustOnHand(2));
        stock.AdjustOnHand(3);
        Assert.Equal(0, stock.Available);
    }

    [Fact]
    public void CompleteLowersOnHandAndReservedOnce()
    {
        var stock = StockItem.Create(ProductId, 5);
        var orderId = Guid.CreateVersion7();
        stock.Reserve(orderId, 2);

        stock.CompleteReservation(orderId);
        stock.CompleteReservation(orderId);

        Assert.Equal(3, stock.OnHand);
        Assert.Equal(0, stock.Reserved);
        Assert.Equal(StockMovementStatus.Completed, Assert.Single(stock.Movements).Status);
        Assert.Single(stock.DomainEvents, domainEvent => domainEvent is StockCompleted);
    }

    [Fact]
    public void ReleaseLowersReservedOnce()
    {
        var stock = StockItem.Create(ProductId, 5);
        var orderId = Guid.CreateVersion7();
        stock.Reserve(orderId, 2);

        stock.ReleaseReservation(orderId);
        stock.ReleaseReservation(orderId);

        Assert.Equal(5, stock.OnHand);
        Assert.Equal(0, stock.Reserved);
        Assert.False(stock.HasActiveReservation(orderId));
        Assert.Single(stock.DomainEvents, domainEvent => domainEvent is StockReleased);
    }

    [Fact]
    public void AReleasedReservationCannotBeCompleted()
    {
        var stock = StockItem.Create(ProductId, 5);
        var orderId = Guid.CreateVersion7();
        stock.Reserve(orderId, 2);
        stock.ReleaseReservation(orderId);

        ErrorAssert.Throws(ErrorCode.Conflict, CatalogMessageKeys.ReservationReleased, () => stock.CompleteReservation(orderId));
        Assert.Equal(5, stock.OnHand);
    }

    [Fact]
    public void ReservedEqualsTheSumOfReservedMovements()
    {
        var stock = StockItem.Create(ProductId, 20);
        var paid = Guid.CreateVersion7();
        var expired = Guid.CreateVersion7();
        stock.Reserve(paid, 3);
        stock.Reserve(expired, 4);
        stock.Reserve(Guid.CreateVersion7(), 5);

        stock.CompleteReservation(paid);
        stock.ReleaseReservation(expired);

        Assert.Equal(
            stock.Movements.Where(movement => movement.Status == StockMovementStatus.Reserved).Sum(movement => movement.Quantity),
            stock.Reserved);
    }

    [Fact]
    public async Task ManagerReservesAllProductsOrStopsAtTheFirstShortage()
    {
        var enough = StockItem.Create(Guid.CreateVersion7(), 5);
        var tooLittle = StockItem.Create(Guid.CreateVersion7(), 1);
        var manager = new StockManager(new StockRepository([enough, tooLittle]), new InMemoryQueryableExecuter());
        var quantities = new Dictionary<Guid, int> { [enough.ProductId] = 2, [tooLittle.ProductId] = 2 };

        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, CatalogMessageKeys.InsufficientStock, () => manager.ReserveForOrderAsync(Guid.CreateVersion7(), quantities, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ManagerCompletesEveryReservationOfAnOrder()
    {
        var first = StockItem.Create(Guid.CreateVersion7(), 5);
        var second = StockItem.Create(Guid.CreateVersion7(), 5);
        var manager = new StockManager(new StockRepository([first, second]), new InMemoryQueryableExecuter());
        var orderId = Guid.CreateVersion7();
        var quantities = new Dictionary<Guid, int> { [first.ProductId] = 1, [second.ProductId] = 2 };

        await manager.ReserveForOrderAsync(orderId, quantities, TestContext.Current.CancellationToken);
        await manager.CompleteForOrderAsync(orderId, TestContext.Current.CancellationToken);

        Assert.Equal(4, first.OnHand);
        Assert.Equal(3, second.OnHand);
    }

    [Fact]
    public async Task ManagerRejectsASecondStockItemForAProduct()
    {
        var existing = StockItem.Create(ProductId, 1);
        var manager = new StockManager(new StockRepository([existing]), new InMemoryQueryableExecuter());

        await ErrorAssert.ThrowsAsync(ErrorCode.Conflict, CatalogMessageKeys.StockItemAlreadyExists, () => manager.CreateAsync(ProductId, 1, TestContext.Current.CancellationToken));
    }

    private sealed class StockRepository(List<StockItem> stockItems) : IStockItemRepository
    {
        public IQueryable<StockItem> GetQueryable() => stockItems.AsQueryable();
    }
}
