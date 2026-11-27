namespace OrderFlow.Domain.Shared.Orders;

/// <summary>
/// The lifecycle state of an order (ADR-0001): Placed, then Confirmed, then Paid or Cancelled.
/// Paid and Cancelled are final. Stored as a tinyint: the numbers are explicit and never reordered.
/// </summary>
public enum OrderStatus : byte
{
    /// <summary>The order exists; its stock is not reserved yet.</summary>
    Placed = 1,

    /// <summary>The stock of every product is reserved; the order waits for payment.</summary>
    Confirmed = 2,

    /// <summary>The payment completed. Final.</summary>
    Paid = 3,

    /// <summary>The order ended before payment (for example an expired reservation). Final, never reopened.</summary>
    Cancelled = 4,
}
