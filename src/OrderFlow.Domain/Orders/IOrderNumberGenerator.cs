namespace OrderFlow.Domain.Orders;

/// <summary>
/// Produces the next unique order number. The implementation and the number format are decided with
/// the persistence milestone.
/// </summary>
public interface IOrderNumberGenerator
{
    /// <summary>Returns a new order number that no other order has.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<string> GenerateAsync(CancellationToken cancellationToken = default);
}
