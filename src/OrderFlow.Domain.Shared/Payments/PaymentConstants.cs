namespace OrderFlow.Domain.Shared.Payments;

/// <summary>Limits of the Payments model.</summary>
public static class PaymentConstants
{
    /// <summary>The maximum length of a provider reference.</summary>
    public const int ProviderReferenceMaxLength = 100;

    /// <summary>The maximum length of a failure reason; longer reasons are cut.</summary>
    public const int FailureReasonMaxLength = 500;
}
