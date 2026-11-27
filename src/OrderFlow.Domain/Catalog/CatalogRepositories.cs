using Domain.Shared.Layer.Repositories;

namespace OrderFlow.Domain.Catalog;

/// <summary>The repository of <see cref="Product"/>. Implemented in EntityFrameworkCore.</summary>
public interface IProductRepository : IRepository<Product>;

/// <summary>The repository of <see cref="StockItem"/>. Implemented in EntityFrameworkCore.</summary>
public interface IStockItemRepository : IRepository<StockItem>;
