using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Catalog.Events;
using OrderFlow.Domain.Shared.Catalog;

namespace OrderFlow.Domain.Catalog;

/// <summary>
/// Something that can be sold: the aggregate root for names and prices (glossary).
/// Invariants: one price per currency; a price is never negative (<see cref="Money"/>);
/// <see cref="NormalizedName"/> always follows <see cref="Name"/> (ADR-0014).
/// SKU uniqueness spans products and is checked by <see cref="ProductManager"/>.
/// </summary>
public sealed class Product : AggregateRoot
{
    private readonly List<ProductPrice> _prices = [];

    private Product(Guid id, Sku sku, string name)
        : base(id)
    {
        Sku = sku;
        Name = name;
        NormalizedName = ProductNameNormalizer.Normalize(name);
    }

    /// <summary>The unique business code.</summary>
    public Sku Sku { get; private set; }

    /// <summary>The name as written.</summary>
    public string Name { get; private set; }

    /// <summary>The name normalized for prefix search (ADR-0014).</summary>
    public string NormalizedName { get; private set; }

    /// <summary>The prices, one per currency.</summary>
    public IReadOnlyList<ProductPrice> Prices => _prices.AsReadOnly();

    /// <summary>Creates a product. Prefer <see cref="ProductManager.CreateAsync"/>, which checks the SKU.</summary>
    /// <param name="sku">The SKU.</param>
    /// <param name="name">The name.</param>
    public static Product Create(Sku sku, string? name)
    {
        ArgumentNullException.ThrowIfNull(sku);

        return new Product(Guid.CreateVersion7(), sku, ValidName(name));
    }

    /// <summary>Changes the name and its normalized form.</summary>
    /// <param name="name">The new name.</param>
    public void Rename(string? name)
    {
        Name = ValidName(name);
        NormalizedName = ProductNameNormalizer.Normalize(Name);
    }

    /// <summary>
    /// Sets the price in its currency: adds it, or changes the existing one. Raises
    /// <see cref="ProductPriceChanged"/> when the amount really changes.
    /// </summary>
    /// <param name="price">The price.</param>
    public void SetPrice(Money price)
    {
        ArgumentNullException.ThrowIfNull(price);

        var existing = _prices.Find(item => item.Price.HasSameCurrency(price));
        var previous = existing?.Price;

        if (previous == price)
        {
            return;
        }

        if (existing is null)
        {
            _prices.Add(new ProductPrice(Guid.CreateVersion7(), price));
        }
        else
        {
            existing.ChangeTo(price);
        }

        AddDomainEvent(new ProductPriceChanged(Id, price.Currency, previous?.Amount, price.Amount));
    }

    /// <summary>Returns the price in a currency, or null when the product has none.</summary>
    /// <param name="currency">The currency code.</param>
    public Money? GetPrice(string? currency)
    {
        var code = Money.NormalizeCurrency(currency);

        return _prices.Find(item => string.Equals(item.Price.Currency, code, StringComparison.Ordinal))?.Price;
    }

    private static string ValidName(string? name) =>
        Check.Text(name, nameof(Name), CatalogConstants.ProductNameMaxLength, CatalogMessageKeys.FieldRequired, CatalogMessageKeys.FieldTooLong);
}
