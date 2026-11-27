using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Localization;

namespace OrderFlow.Domain.Shared.Payments;

/// <summary>Message keys of the Payments module (ADR-0021).</summary>
public static class PaymentsMessageKeys
{
    /// <summary>A required field is empty. Arguments: the field name.</summary>
    public static readonly MessageKey FieldRequired = new(Modules.Payments, nameof(FieldRequired));

    /// <summary>A field is too long. Arguments: the field name, the maximum length.</summary>
    public static readonly MessageKey FieldTooLong = new(Modules.Payments, nameof(FieldTooLong));

    /// <summary>A payment amount is zero. Arguments: the amount.</summary>
    public static readonly MessageKey AmountNotPositive = new(Modules.Payments, nameof(AmountNotPositive));

    /// <summary>The payment no longer accepts attempts. Arguments: the payment id, its status.</summary>
    public static readonly MessageKey PaymentNotOpen = new(Modules.Payments, nameof(PaymentNotOpen));

    /// <summary>Only a completed payment can be refunded. Arguments: the payment id, its status.</summary>
    public static readonly MessageKey PaymentNotCompleted = new(Modules.Payments, nameof(PaymentNotCompleted));

    /// <summary>The order already has a completed payment. Arguments: the order id.</summary>
    public static readonly MessageKey OrderAlreadyPaid = new(Modules.Payments, nameof(OrderAlreadyPaid));
}
