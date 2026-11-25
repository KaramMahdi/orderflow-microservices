namespace Domain.Shared.Layer.Constants;

/// <summary>
/// Module names: the bounded contexts (docs/context-map.md) and Common, the module of the generic
/// messages used as the last fallback (ADR-0021). Used in <see cref="Localization.MessageKey"/>.
/// </summary>
public static class Modules
{
    /// <summary>Generic messages shared by every module.</summary>
    public const string Common = nameof(Common);

    /// <summary>The Orders context.</summary>
    public const string Orders = nameof(Orders);

    /// <summary>The Catalog context.</summary>
    public const string Catalog = nameof(Catalog);

    /// <summary>The Customers context.</summary>
    public const string Customers = nameof(Customers);

    /// <summary>The Payments context.</summary>
    public const string Payments = nameof(Payments);

    /// <summary>The Notifications context.</summary>
    public const string Notifications = nameof(Notifications);

    /// <summary>The Identity context.</summary>
    public const string Identity = nameof(Identity);

    /// <summary>The Files context.</summary>
    public const string Files = nameof(Files);
}
