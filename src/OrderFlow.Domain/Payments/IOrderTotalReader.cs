using Domain.Shared.Layer.ValueObjects;

namespace OrderFlow.Domain.Payments;

/// <summary>
/// Reads the total of an order for Payments, so a payment amount always equals the order total
/// (decided by Karam, 2026-11-28). Payments never loads the Order aggregate. In Phase 1 the Orders
/// module implements it in process; from Phase 4 the source is revisited with R-016 and R-017.
/// </summary>
public interface IOrderTotalReader
{
    /// <summary>Returns the order total, or null when the order does not exist.</summary>
    /// <param name="orderId">The order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<Money?> GetTotalAsync(Guid orderId, CancellationToken cancellationToken = default);
}
