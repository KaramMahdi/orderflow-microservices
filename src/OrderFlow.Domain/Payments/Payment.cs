using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Payments.Events;
using OrderFlow.Domain.Shared.Payments;

namespace OrderFlow.Domain.Payments;

/// <summary>
/// An attempt to collect the total of one order (glossary), with its tries.
/// Invariants: the amount is the order total (only <see cref="PaymentManager"/> starts a payment, with
/// the total it reads) and above zero; the collected amount equals the amount; no attempt after
/// completion or refund; only a completed payment is refunded, once. "One successful payment per
/// order" spans payments and is checked by <see cref="PaymentManager"/>.
/// </summary>
public sealed class Payment : AggregateRoot
{
    private readonly List<PaymentAttempt> _attempts = [];

    private Payment(Guid id, Guid orderId, Money amount)
        : base(id)
    {
        OrderId = orderId;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    /// <summary>The order.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>The amount to collect: the order total.</summary>
    public Money Amount { get; private set; }

    /// <summary>The state.</summary>
    public PaymentStatus Status { get; private set; }

    /// <summary>The provider id of the successful attempt; unique when present.</summary>
    public string? ProviderReference { get; private set; }

    /// <summary>The tries, in order.</summary>
    public IReadOnlyList<PaymentAttempt> Attempts => _attempts.AsReadOnly();

    /// <summary>Starts a payment for an order. Called only by <see cref="PaymentManager.StartAsync"/>.</summary>
    /// <param name="orderId">The order.</param>
    /// <param name="amount">The order total.</param>
    internal static Payment Start(Guid orderId, Money amount)
    {
        ArgumentNullException.ThrowIfNull(amount);
        Check.NotEmpty(orderId, nameof(OrderId), PaymentsMessageKeys.FieldRequired);

        if (amount.Amount <= 0)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, PaymentsMessageKeys.AmountNotPositive, amount.Amount);
        }

        return new Payment(Guid.CreateVersion7(), orderId, amount);
    }

    /// <summary>The provider collected the money. Raises <see cref="PaymentCompleted"/>. Prefer <see cref="PaymentManager.CompleteAsync"/>.</summary>
    /// <param name="providerReference">The provider id of the payment.</param>
    /// <param name="collectedAmount">The amount the provider collected; must equal <see cref="Amount"/>.</param>
    public void RecordSuccessfulAttempt(string? providerReference, Money collectedAmount)
    {
        ArgumentNullException.ThrowIfNull(collectedAmount);
        EnsureOpen();
        var reference = Check.Text(providerReference, nameof(ProviderReference), PaymentConstants.ProviderReferenceMaxLength, PaymentsMessageKeys.FieldRequired, PaymentsMessageKeys.FieldTooLong);

        if (collectedAmount != Amount)
        {
            throw new ErrorException(ErrorCode.Conflict, PaymentsMessageKeys.CollectedAmountMismatch, Id, Amount.ToString(), collectedAmount.ToString());
        }

        _attempts.Add(new PaymentAttempt(Guid.CreateVersion7(), PaymentAttemptResult.Succeeded, reference, failureReason: null));
        ProviderReference = reference;
        Status = PaymentStatus.Completed;
        AddDomainEvent(new PaymentCompleted(Id, OrderId, Amount));
    }

    /// <summary>The provider did not collect the money. Raises <see cref="PaymentFailed"/>. Another attempt may follow.</summary>
    /// <param name="providerReference">The provider id of the attempt, when there is one.</param>
    /// <param name="failureReason">Why it failed, when known; cut to the maximum length.</param>
    public void RecordFailedAttempt(string? providerReference, string? failureReason)
    {
        EnsureOpen();
        var reference = Check.OptionalText(providerReference, nameof(ProviderReference), PaymentConstants.ProviderReferenceMaxLength, PaymentsMessageKeys.FieldTooLong);
        var reason = failureReason?.Trim() is { Length: > 0 } trimmed
            ? trimmed[..Math.Min(trimmed.Length, PaymentConstants.FailureReasonMaxLength)]
            : null;

        _attempts.Add(new PaymentAttempt(Guid.CreateVersion7(), PaymentAttemptResult.Failed, reference, reason));
        Status = PaymentStatus.Failed;
        AddDomainEvent(new PaymentFailed(Id, OrderId));
    }

    /// <summary>
    /// Returns the collected money (late payment, ADR-0001). Raises <see cref="PaymentRefunded"/>.
    /// Refunding twice does nothing (idempotent).
    /// </summary>
    public void Refund()
    {
        if (Status == PaymentStatus.Refunded)
        {
            return;
        }

        if (Status != PaymentStatus.Completed)
        {
            throw new ErrorException(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotCompleted, Id, Status);
        }

        Status = PaymentStatus.Refunded;
        AddDomainEvent(new PaymentRefunded(Id, OrderId, Amount));
    }

    private void EnsureOpen()
    {
        if (Status is not (PaymentStatus.Pending or PaymentStatus.Failed))
        {
            throw new ErrorException(ErrorCode.Conflict, PaymentsMessageKeys.PaymentNotOpen, Id, Status);
        }
    }
}
