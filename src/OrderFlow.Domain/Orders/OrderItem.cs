using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.ValueObjects;

namespace OrderFlow.Domain.Orders;

/// <summary>
/// One product line of an order: product, quantity, unit price (glossary). Lives only inside an
/// <see cref="Order"/>, which validates it.
/// </summary>
public sealed class OrderItem : Entity
{
    internal OrderItem(Guid id, Guid productId, int quantity, Money unitPrice)
        : base(id)
    {
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    /// <summary>The product (a Catalog product id).</summary>
    public Guid ProductId { get; private set; }

    /// <summary>The quantity, above zero.</summary>
    public int Quantity { get; private set; }

    /// <summary>The price of one unit at order time, in the order currency.</summary>
    public Money UnitPrice { get; private set; }

    /// <summary>The unit price times the quantity.</summary>
    public Money LineTotal => UnitPrice.Multiply(Quantity);
}
