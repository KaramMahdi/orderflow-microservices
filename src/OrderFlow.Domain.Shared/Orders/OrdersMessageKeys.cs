using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Localization;

namespace OrderFlow.Domain.Shared.Orders;

/// <summary>Message keys of the Orders module (ADR-0021).</summary>
public static class OrdersMessageKeys
{
    /// <summary>A required field is empty. Arguments: the field name.</summary>
    public static readonly MessageKey FieldRequired = new(Modules.Orders, nameof(FieldRequired));

    /// <summary>A field is too long. Arguments: the field name, the maximum length.</summary>
    public static readonly MessageKey FieldTooLong = new(Modules.Orders, nameof(FieldTooLong));

    /// <summary>An order was placed without items.</summary>
    public static readonly MessageKey OrderHasNoItems = new(Modules.Orders, nameof(OrderHasNoItems));

    /// <summary>An item quantity is zero or negative. Arguments: the product id, the quantity.</summary>
    public static readonly MessageKey QuantityNotPositive = new(Modules.Orders, nameof(QuantityNotPositive));

    /// <summary>An item price is not in the order currency. Arguments: the product id, the item currency, the order currency.</summary>
    public static readonly MessageKey ItemCurrencyMismatch = new(Modules.Orders, nameof(ItemCurrencyMismatch));

    /// <summary>The same product appears twice in one order. Arguments: the product id.</summary>
    public static readonly MessageKey DuplicateProduct = new(Modules.Orders, nameof(DuplicateProduct));

    /// <summary>A status change that the lifecycle does not allow. Arguments: the current status, the requested status.</summary>
    public static readonly MessageKey InvalidStatusTransition = new(Modules.Orders, nameof(InvalidStatusTransition));
}
