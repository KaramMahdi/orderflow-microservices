using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.ValueObjects;

namespace OrderFlow.Domain.Payments.Events;

/// <summary>The money of a payment was collected.</summary>
/// <param name="PaymentId">The payment.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Amount">The amount.</param>
public sealed record PaymentCompleted(Guid PaymentId, Guid OrderId, Money Amount) : IDomainEvent;

/// <summary>An attempt to collect the money failed.</summary>
/// <param name="PaymentId">The payment.</param>
/// <param name="OrderId">The order.</param>
public sealed record PaymentFailed(Guid PaymentId, Guid OrderId) : IDomainEvent;

/// <summary>A collected payment was returned (late payment, ADR-0001).</summary>
/// <param name="PaymentId">The payment.</param>
/// <param name="OrderId">The order.</param>
/// <param name="Amount">The amount.</param>
public sealed record PaymentRefunded(Guid PaymentId, Guid OrderId, Money Amount) : IDomainEvent;
