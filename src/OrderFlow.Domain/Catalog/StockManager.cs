using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Repositories;
using OrderFlow.Domain.Catalog.Specifications;
using OrderFlow.Domain.Shared.Catalog;

namespace OrderFlow.Domain.Catalog;

/// <summary>
/// Stock rules that span several stock items (ADR-0001): one stock item per product, and an order
/// reserves all its products or none, in ProductId order. It changes the loaded aggregates and does
/// not save; the Application Service saves everything in one transaction, so a failure saves nothing.
/// </summary>
/// <param name="stockItemRepository">The stock item repository.</param>
/// <param name="queryableExecuter">Executes the queries.</param>
public sealed class StockManager(IStockItemRepository stockItemRepository, IAsyncQueryableExecuter queryableExecuter)
{
    /// <summary>Creates the stock of a product after checking it has none.</summary>
    /// <param name="productId">The product.</param>
    /// <param name="onHand">The initial quantity on hand.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<StockItem> CreateAsync(Guid productId, int onHand, CancellationToken cancellationToken = default)
    {
        var query = stockItemRepository.GetQueryable()
            .Where(new StockItemsOfProductsSpecification([productId]).ToExpression());

        if (await queryableExecuter.CountAsync(query, cancellationToken).ConfigureAwait(false) > 0)
        {
            throw new ErrorException(ErrorCode.Conflict, CatalogMessageKeys.StockItemAlreadyExists, productId);
        }

        return StockItem.Create(productId, onHand);
    }

    /// <summary>Reserves every product of an order, in ProductId order. Throws on the first product that cannot be reserved.</summary>
    /// <param name="orderId">The order.</param>
    /// <param name="quantitiesByProduct">The quantity of each product.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<IReadOnlyList<StockItem>> ReserveForOrderAsync(
        Guid orderId,
        IReadOnlyDictionary<Guid, int> quantitiesByProduct,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(quantitiesByProduct);

        var productIds = quantitiesByProduct.Keys.Order().ToList();
        var query = stockItemRepository.GetQueryable()
            .Where(new StockItemsOfProductsSpecification(productIds).ToExpression());
        var stockItems = await queryableExecuter.ToListAsync(query, cancellationToken).ConfigureAwait(false);
        var reserved = new List<StockItem>(productIds.Count);

        foreach (var productId in productIds)
        {
            var stockItem = stockItems.Find(item => item.ProductId == productId)
                ?? throw new ErrorException(ErrorCode.NotFound, CatalogMessageKeys.StockItemNotFound, productId);

            stockItem.Reserve(orderId, quantitiesByProduct[productId]);
            reserved.Add(stockItem);
        }

        return reserved;
    }

    /// <summary>Completes the reservations of a paid order. See <see cref="StockItem.CompleteReservation"/>.</summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<IReadOnlyList<StockItem>> CompleteForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var stockItems = await GetForOrderAsync(orderId, cancellationToken).ConfigureAwait(false);

        foreach (var stockItem in stockItems)
        {
            stockItem.CompleteReservation(orderId);
        }

        return stockItems;
    }

    /// <summary>Releases the reservations of an order. See <see cref="StockItem.ReleaseReservation"/>.</summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task<IReadOnlyList<StockItem>> ReleaseForOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var stockItems = await GetForOrderAsync(orderId, cancellationToken).ConfigureAwait(false);

        foreach (var stockItem in stockItems)
        {
            stockItem.ReleaseReservation(orderId);
        }

        return stockItems;
    }

    private async Task<List<StockItem>> GetForOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var query = stockItemRepository.GetQueryable()
            .Where(new StockItemsOfOrderSpecification(orderId).ToExpression())
            .OrderBy(stockItem => stockItem.ProductId);

        return await queryableExecuter.ToListAsync(query, cancellationToken).ConfigureAwait(false);
    }
}
