using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Repositories;
using OrderFlow.Domain.Catalog.Specifications;
using OrderFlow.Domain.Shared.Catalog;

namespace OrderFlow.Domain.Catalog;

/// <summary>
/// Creates products with a SKU no other product has (a rule across products, so it needs the
/// repository). The unique index on Sku is the safety net for concurrent creations.
/// It does not save; the Application Service saves through the unit of work.
/// </summary>
/// <param name="productRepository">The product repository.</param>
/// <param name="queryableExecuter">Executes the query.</param>
public sealed class ProductManager(IProductRepository productRepository, IAsyncQueryableExecuter queryableExecuter)
{
    /// <summary>Creates a product after checking its SKU is free.</summary>
    /// <param name="sku">The SKU.</param>
    /// <param name="name">The name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<Product> CreateAsync(string? sku, string? name, CancellationToken cancellationToken = default)
    {
        var validSku = Sku.Create(sku);
        var query = productRepository.GetQueryable().Where(new ProductBySkuSpecification(validSku).ToExpression());

        if (await queryableExecuter.CountAsync(query, cancellationToken).ConfigureAwait(false) > 0)
        {
            throw new ErrorException(ErrorCode.Conflict, CatalogMessageKeys.SkuAlreadyExists, validSku.Value);
        }

        return Product.Create(validSku, name);
    }
}
