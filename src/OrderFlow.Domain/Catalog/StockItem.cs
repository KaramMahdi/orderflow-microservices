using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using OrderFlow.Domain.Catalog.Events;
using OrderFlow.Domain.Shared.Catalog;

namespace OrderFlow.Domain.Catalog;

/// <summary>
/// The stock of one product: on hand and reserved (glossary, ADR-0001). The aggregate root for stock.
/// Invariants: quantities never negative; Reserved never above OnHand; Reserved equals the sum of the
/// Reserved movements; one reservation per order. Completing a movement lowers OnHand and Reserved;
/// releasing it (Failed) lowers Reserved.
/// </summary>
public sealed class StockItem : AggregateRoot
{
    private readonly List<StockMovement> _movements = [];

    private StockItem(Guid id, Guid productId, int onHand)
        : base(id)
    {
        ProductId = productId;
        OnHand = onHand;
    }

    /// <summary>The product (one stock item per product).</summary>
    public Guid ProductId { get; private set; }

    /// <summary>The quantity physically in stock.</summary>
    public int OnHand { get; private set; }

    /// <summary>The quantity held by Reserved movements.</summary>
    public int Reserved { get; private set; }

    /// <summary>What can still be ordered: on hand minus reserved.</summary>
    public int Available => OnHand - Reserved;

    /// <summary>The movements of this stock.</summary>
    public IReadOnlyList<StockMovement> Movements => _movements.AsReadOnly();

    /// <summary>Creates the stock of a product. Prefer <see cref="StockManager.CreateAsync"/>, which checks there is no other.</summary>
    /// <param name="productId">The product.</param>
    /// <param name="onHand">The initial quantity, not negative.</param>
    public static StockItem Create(Guid productId, int onHand)
    {
        Check.NotEmpty(productId, nameof(ProductId), CatalogMessageKeys.FieldRequired);
        EnsureNotNegative(onHand);

        return new StockItem(Guid.CreateVersion7(), productId, onHand);
    }

    /// <summary>Sets the quantity on hand (a stock count or a delivery). Never below Reserved.</summary>
    /// <param name="onHand">The new quantity on hand.</param>
    public void AdjustOnHand(int onHand)
    {
        EnsureNotNegative(onHand);

        if (onHand < Reserved)
        {
            throw new ErrorException(ErrorCode.Conflict, CatalogMessageKeys.OnHandBelowReserved, onHand, Reserved);
        }

        OnHand = onHand;
    }

    /// <summary>Holds a quantity for an order: adds a Reserved movement and raises <see cref="StockReserved"/>.</summary>
    /// <param name="orderId">The order.</param>
    /// <param name="quantity">The quantity, above zero and not above <see cref="Available"/>.</param>
    public StockMovement Reserve(Guid orderId, int quantity)
    {
        Check.NotEmpty(orderId, nameof(StockMovement.OrderId), CatalogMessageKeys.FieldRequired);

        if (quantity <= 0)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CatalogMessageKeys.QuantityNotPositive, quantity);
        }

        if (FindMovement(orderId) is not null)
        {
            throw new ErrorException(ErrorCode.Conflict, CatalogMessageKeys.ReservationAlreadyExists, ProductId, orderId);
        }

        if (quantity > Available)
        {
            throw new ErrorException(ErrorCode.Conflict, CatalogMessageKeys.InsufficientStock, ProductId, Available, quantity);
        }

        var movement = new StockMovement(Guid.CreateVersion7(), ProductId, orderId, quantity);
        _movements.Add(movement);
        Reserved += quantity;
        AddDomainEvent(new StockReserved(Id, ProductId, orderId, quantity));

        return movement;
    }

    /// <summary>
    /// The order was paid: the reserved quantity leaves the stock. Raises <see cref="StockCompleted"/>.
    /// Completing twice does nothing (idempotent). A released reservation cannot be completed: the
    /// payment is late and is refunded (ADR-0001).
    /// </summary>
    /// <param name="orderId">The order.</param>
    public void CompleteReservation(Guid orderId)
    {
        var movement = GetMovement(orderId);

        switch (movement.Status)
        {
            case StockMovementStatus.Completed:
                return;
            case StockMovementStatus.Failed:
                throw new ErrorException(ErrorCode.Conflict, CatalogMessageKeys.ReservationReleased, ProductId, orderId);
        }

        movement.Complete();
        OnHand -= movement.Quantity;
        Reserved -= movement.Quantity;
        AddDomainEvent(new StockCompleted(Id, ProductId, orderId, movement.Quantity));
    }

    /// <summary>
    /// Releases the reservation of an order (not paid in time): the movement becomes Failed and
    /// <see cref="StockReleased"/> is raised. Does nothing when the movement is no longer Reserved (idempotent).
    /// </summary>
    /// <param name="orderId">The order.</param>
    public void ReleaseReservation(Guid orderId)
    {
        var movement = GetMovement(orderId);

        if (movement.Status != StockMovementStatus.Reserved)
        {
            return;
        }

        movement.Fail();
        Reserved -= movement.Quantity;
        AddDomainEvent(new StockReleased(Id, ProductId, orderId, movement.Quantity));
    }

    /// <summary>Returns true when the order holds a Reserved movement of this stock.</summary>
    /// <param name="orderId">The order.</param>
    public bool HasActiveReservation(Guid orderId) =>
        FindMovement(orderId)?.Status == StockMovementStatus.Reserved;

    private static void EnsureNotNegative(int quantity)
    {
        if (quantity < 0)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CatalogMessageKeys.QuantityNegative, quantity);
        }
    }

    private StockMovement? FindMovement(Guid orderId) => _movements.Find(movement => movement.OrderId == orderId);

    private StockMovement GetMovement(Guid orderId) =>
        FindMovement(orderId)
        ?? throw new ErrorException(ErrorCode.NotFound, CatalogMessageKeys.ReservationNotFound, ProductId, orderId);
}
