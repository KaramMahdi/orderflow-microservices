using System.Linq.Expressions;
using Domain.Shared.Layer.Specifications;

namespace OrderFlow.Domain.Catalog.Specifications;

/// <summary>The product with a SKU.</summary>
/// <param name="sku">The SKU.</param>
public sealed class ProductBySkuSpecification(Sku sku) : Specification<Product>
{
    /// <inheritdoc />
    public override Expression<Func<Product, bool>> ToExpression() => product => product.Sku == sku;
}

/// <summary>
/// The products whose normalized name starts with a prefix: <c>LIKE 'abc%'</c> on NormalizedName (ADR-0014).
/// The prefix is normalized here with <see cref="ProductNameNormalizer"/>.
/// </summary>
/// <param name="prefix">The search text.</param>
public sealed class ProductNameStartsWithSpecification(string prefix) : Specification<Product>
{
    private readonly string _normalizedPrefix = ProductNameNormalizer.Normalize(prefix.Trim());

    /// <inheritdoc />
    public override Expression<Func<Product, bool>> ToExpression()
    {
        var normalizedPrefix = _normalizedPrefix;

        return product => product.NormalizedName.StartsWith(normalizedPrefix);
    }
}

/// <summary>The stock items of a set of products.</summary>
/// <param name="productIds">The products.</param>
public sealed class StockItemsOfProductsSpecification(IReadOnlyCollection<Guid> productIds) : Specification<StockItem>
{
    /// <inheritdoc />
    public override Expression<Func<StockItem, bool>> ToExpression() => stockItem => productIds.Contains(stockItem.ProductId);
}

/// <summary>The stock items that hold a movement for an order.</summary>
/// <param name="orderId">The order.</param>
public sealed class StockItemsOfOrderSpecification(Guid orderId) : Specification<StockItem>
{
    /// <inheritdoc />
    public override Expression<Func<StockItem, bool>> ToExpression() =>
        stockItem => stockItem.Movements.Any(movement => movement.OrderId == orderId);
}
