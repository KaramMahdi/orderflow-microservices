namespace OrderFlow.Domain.Shared.Catalog;

/// <summary>
/// The state of a stock movement (ADR-0001): Reserved, then Completed or Failed.
/// Stored as a tinyint: the numbers are explicit and never reordered.
/// </summary>
public enum StockMovementStatus : byte
{
    /// <summary>The quantity is held for an order and counted in the Reserved counter.</summary>
    Reserved = 1,

    /// <summary>The order was paid: the quantity left the stock.</summary>
    Completed = 2,

    /// <summary>The reservation was released (not paid in time).</summary>
    Failed = 3,
}
