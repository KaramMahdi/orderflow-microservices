using Domain.Shared.Layer.DomainModel;
using OrderFlow.Domain.Shared.Catalog;

namespace OrderFlow.Domain.Catalog;

/// <summary>
/// A quantity held or taken for an order: Reserved, then Completed or Failed (glossary, ADR-0001).
/// Lives only inside a <see cref="StockItem"/>, which keeps its Reserved counter in step.
/// </summary>
public sealed class StockMovement : Entity
{
    internal StockMovement(Guid id, Guid productId, Guid orderId, int quantity)
        : base(id)
    {
        ProductId = productId;
        OrderId = orderId;
        Quantity = quantity;
        Status = StockMovementStatus.Reserved;
    }

    /// <summary>The product.</summary>
    public Guid ProductId { get; private set; }

    /// <summary>The order the quantity is held for.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>The quantity, above zero.</summary>
    public int Quantity { get; private set; }

    /// <summary>The state.</summary>
    public StockMovementStatus Status { get; private set; }

    internal void Complete() => Status = StockMovementStatus.Completed;

    internal void Fail() => Status = StockMovementStatus.Failed;
}
