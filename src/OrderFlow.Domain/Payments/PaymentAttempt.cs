using Domain.Shared.Layer.DomainModel;
using OrderFlow.Domain.Shared.Payments;

namespace OrderFlow.Domain.Payments;

/// <summary>One try to pay (glossary). Lives only inside a <see cref="Payment"/>; its time is the CreationTime audit column.</summary>
public sealed class PaymentAttempt : Entity
{
    internal PaymentAttempt(Guid id, PaymentAttemptResult result, string? providerReference, string? failureReason)
        : base(id)
    {
        Result = result;
        ProviderReference = providerReference;
        FailureReason = failureReason;
    }

    /// <summary>The outcome.</summary>
    public PaymentAttemptResult Result { get; private set; }

    /// <summary>The provider id of the attempt, when there is one.</summary>
    public string? ProviderReference { get; private set; }

    /// <summary>Why the attempt failed, when known.</summary>
    public string? FailureReason { get; private set; }
}
