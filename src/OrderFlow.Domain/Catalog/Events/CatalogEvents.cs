using Domain.Shared.Layer.DomainModel;

namespace OrderFlow.Domain.Catalog.Events;

/// <summary>The price of a product in a currency was added or changed.</summary>
/// <param name="ProductId">The product.</param>
/// <param name="Currency">The currency.</param>
/// <param name="PreviousAmount">The previous amount, null when the price is new.</param>
/// <param name="NewAmount">The new amount.</param>
public sealed record ProductPriceChanged(Guid ProductId, string Currency, decimal? PreviousAmount, decimal NewAmount) : IDomainEvent;

/// <summary>A quantity was reserved for an order.</summary>
/// <param name="StockItemId">The stock item.</param>
/// <param name="ProductId">The product.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Quantity">The quantity.</param>
public sealed record StockReserved(Guid StockItemId, Guid ProductId, Guid OrderId, int Quantity) : IDomainEvent;

/// <summary>A reservation was completed: the quantity left the stock.</summary>
/// <param name="StockItemId">The stock item.</param>
/// <param name="ProductId">The product.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Quantity">The quantity.</param>
public sealed record StockCompleted(Guid StockItemId, Guid ProductId, Guid OrderId, int Quantity) : IDomainEvent;

/// <summary>A reservation was released (Failed).</summary>
/// <param name="StockItemId">The stock item.</param>
/// <param name="ProductId">The product.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Quantity">The quantity.</param>
public sealed record StockReleased(Guid StockItemId, Guid ProductId, Guid OrderId, int Quantity) : IDomainEvent;
