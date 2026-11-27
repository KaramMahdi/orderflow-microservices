using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.ValueObjects;

namespace OrderFlow.Domain.Catalog;

/// <summary>The price of a product in one currency (glossary). Lives only inside a <see cref="Product"/>.</summary>
public sealed class ProductPrice : Entity
{
    internal ProductPrice(Guid id, Money price)
        : base(id)
    {
        Price = price;
    }

    /// <summary>The price; its currency never changes.</summary>
    public Money Price { get; private set; }

    internal void ChangeTo(Money price)
    {
        Price.EnsureSameCurrency(price);
        Price = price;
    }
}
