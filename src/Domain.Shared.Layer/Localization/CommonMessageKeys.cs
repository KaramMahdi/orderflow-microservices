using Domain.Shared.Layer.Constants;

namespace Domain.Shared.Layer.Localization;

/// <summary>
/// Message keys of the shared value objects (<see cref="ValueObjects.Money"/>, <see cref="ValueObjects.Address"/>),
/// in the <see cref="Modules.Common"/> module of the Translations table (ADR-0021).
/// </summary>
public static class CommonMessageKeys
{
    /// <summary>A required field is empty. Arguments: the field name.</summary>
    public static readonly MessageKey FieldRequired = new(Modules.Common, nameof(FieldRequired));

    /// <summary>A field is too long. Arguments: the field name, the maximum length.</summary>
    public static readonly MessageKey FieldTooLong = new(Modules.Common, nameof(FieldTooLong));

    /// <summary>A money amount is negative. Arguments: the amount.</summary>
    public static readonly MessageKey MoneyAmountNegative = new(Modules.Common, nameof(MoneyAmountNegative));

    /// <summary>A money amount has more decimal places than allowed. Arguments: the amount, the maximum decimal places.</summary>
    public static readonly MessageKey MoneyAmountTooPrecise = new(Modules.Common, nameof(MoneyAmountTooPrecise));

    /// <summary>A money amount is above the maximum. Arguments: the amount, the maximum.</summary>
    public static readonly MessageKey MoneyAmountTooLarge = new(Modules.Common, nameof(MoneyAmountTooLarge));

    /// <summary>A currency code is not three letters (ISO 4217). Arguments: the code.</summary>
    public static readonly MessageKey CurrencyCodeInvalid = new(Modules.Common, nameof(CurrencyCodeInvalid));

    /// <summary>Two amounts in different currencies were combined. Arguments: the two currency codes.</summary>
    public static readonly MessageKey CurrencyMismatch = new(Modules.Common, nameof(CurrencyMismatch));

    /// <summary>A country code is not two letters (ISO 3166-1 alpha-2). Arguments: the code.</summary>
    public static readonly MessageKey CountryCodeInvalid = new(Modules.Common, nameof(CountryCodeInvalid));
}
