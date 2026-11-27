namespace OrderFlow.Domain.Shared.Payments;

/// <summary>
/// The outcome of one try to pay. Stored as a tinyint: the numbers are explicit and never reordered.
/// </summary>
public enum PaymentAttemptResult : byte
{
    /// <summary>The provider collected the money.</summary>
    Succeeded = 1,

    /// <summary>The provider did not collect the money.</summary>
    Failed = 2,
}
