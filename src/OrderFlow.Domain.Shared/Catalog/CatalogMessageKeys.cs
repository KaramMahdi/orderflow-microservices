using Domain.Shared.Layer.Constants;
using Domain.Shared.Layer.Localization;

namespace OrderFlow.Domain.Shared.Catalog;

/// <summary>Message keys of the Catalog module (ADR-0021).</summary>
public static class CatalogMessageKeys
{
    /// <summary>A required field is empty. Arguments: the field name.</summary>
    public static readonly MessageKey FieldRequired = new(Modules.Catalog, nameof(FieldRequired));

    /// <summary>A field is too long. Arguments: the field name, the maximum length.</summary>
    public static readonly MessageKey FieldTooLong = new(Modules.Catalog, nameof(FieldTooLong));

    /// <summary>A SKU has characters other than letters, digits, '-' and '_'. Arguments: the SKU.</summary>
    public static readonly MessageKey SkuInvalid = new(Modules.Catalog, nameof(SkuInvalid));

    /// <summary>Another product already has the SKU. Arguments: the SKU.</summary>
    public static readonly MessageKey SkuAlreadyExists = new(Modules.Catalog, nameof(SkuAlreadyExists));

    /// <summary>A stock quantity is negative. Arguments: the quantity.</summary>
    public static readonly MessageKey QuantityNegative = new(Modules.Catalog, nameof(QuantityNegative));

    /// <summary>A reserved quantity is zero or negative. Arguments: the quantity.</summary>
    public static readonly MessageKey QuantityNotPositive = new(Modules.Catalog, nameof(QuantityNotPositive));

    /// <summary>On hand would go below Reserved. Arguments: the requested on hand, the reserved quantity.</summary>
    public static readonly MessageKey OnHandBelowReserved = new(Modules.Catalog, nameof(OnHandBelowReserved));

    /// <summary>Not enough available stock. Arguments: the product id, the available quantity, the requested quantity.</summary>
    public static readonly MessageKey InsufficientStock = new(Modules.Catalog, nameof(InsufficientStock));

    /// <summary>The order already has a reservation of this product. Arguments: the product id, the order id.</summary>
    public static readonly MessageKey ReservationAlreadyExists = new(Modules.Catalog, nameof(ReservationAlreadyExists));

    /// <summary>The order has no reservation of this product. Arguments: the product id, the order id.</summary>
    public static readonly MessageKey ReservationNotFound = new(Modules.Catalog, nameof(ReservationNotFound));

    /// <summary>The reservation was already released and cannot be completed. Arguments: the product id, the order id.</summary>
    public static readonly MessageKey ReservationReleased = new(Modules.Catalog, nameof(ReservationReleased));

    /// <summary>The product has no stock item. Arguments: the product id.</summary>
    public static readonly MessageKey StockItemNotFound = new(Modules.Catalog, nameof(StockItemNotFound));

    /// <summary>The product already has a stock item. Arguments: the product id.</summary>
    public static readonly MessageKey StockItemAlreadyExists = new(Modules.Catalog, nameof(StockItemAlreadyExists));
}
