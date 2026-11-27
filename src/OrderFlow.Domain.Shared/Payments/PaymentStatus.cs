namespace OrderFlow.Domain.Shared.Payments;

/// <summary>
/// The state of a payment. Stored as a tinyint: the numbers are explicit and never reordered.
/// </summary>
public enum PaymentStatus : byte
{
    /// <summary>Created; no attempt yet.</summary>
    Pending = 1,

    /// <summary>The money was collected.</summary>
    Completed = 2,

    /// <summary>The last attempt failed; another attempt may follow.</summary>
    Failed = 3,

    /// <summary>The collected money was returned (late payment, ADR-0001). Final.</summary>
    Refunded = 4,
}
