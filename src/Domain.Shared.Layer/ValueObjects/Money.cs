using System.Globalization;
using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;

namespace Domain.Shared.Layer.ValueObjects;

/// <summary>
/// An amount and a currency code (ADR-0003). Immutable, never negative, at most two decimal places
/// (stored as decimal(18,2)); an amount with more decimal places is rejected, never rounded.
/// Amounts in different currencies are never combined: there is no conversion.
/// </summary>
public sealed class Money : ValueObject
{
    /// <summary>The maximum number of decimal places (decimal(18,2), ADR-0003).</summary>
    public const int MaxDecimalPlaces = 2;

    /// <summary>The length of a currency code (ISO 4217).</summary>
    public const int CurrencyCodeLength = 3;

    /// <summary>The largest amount that fits decimal(18,2).</summary>
    public const decimal MaxAmount = 9_999_999_999_999_999.99m;

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>The amount.</summary>
    public decimal Amount { get; }

    /// <summary>The currency code: three upper-case letters (ISO 4217).</summary>
    public string Currency { get; }

    /// <summary>Creates a validated amount.</summary>
    /// <param name="amount">The amount: not negative, at most two decimal places.</param>
    /// <param name="currency">The currency code (ISO 4217, any case).</param>
    public static Money Create(decimal amount, string? currency)
    {
        var code = NormalizeCurrency(currency);

        if (amount < 0)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountNegative, amount);
        }

        if (decimal.Round(amount, MaxDecimalPlaces) != amount)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountTooPrecise, amount, MaxDecimalPlaces);
        }

        if (amount > MaxAmount)
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountTooLarge, amount, MaxAmount);
        }

        return new Money(amount, code);
    }

    /// <summary>Creates a zero amount in the currency.</summary>
    /// <param name="currency">The currency code.</param>
    public static Money Zero(string? currency) => Create(0m, currency);

    /// <summary>Validates a currency code and returns it in upper case.</summary>
    /// <param name="currency">The currency code (ISO 4217, any case).</param>
    public static string NormalizeCurrency(string? currency)
    {
        var code = currency?.Trim().ToUpperInvariant();

        if (code is null || code.Length != CurrencyCodeLength || !code.All(char.IsAsciiLetterUpper))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CommonMessageKeys.CurrencyCodeInvalid, currency);
        }

        return code;
    }

    /// <summary>Returns the sum of both amounts. Both must be in the same currency.</summary>
    /// <param name="other">The amount to add.</param>
    public Money Add(Money other)
    {
        EnsureSameCurrency(other);

        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>Returns the amount multiplied by a quantity.</summary>
    /// <param name="quantity">The quantity, not negative.</param>
    public Money Multiply(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);

        return Create(Amount * quantity, Currency);
    }

    /// <summary>Returns true when both amounts are in the same currency.</summary>
    /// <param name="other">The other amount.</param>
    public bool HasSameCurrency(Money other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return string.Equals(Currency, other.Currency, StringComparison.Ordinal);
    }

    /// <summary>Throws when the other amount is in another currency.</summary>
    /// <param name="other">The other amount.</param>
    public void EnsureSameCurrency(Money other)
    {
        if (!HasSameCurrency(other))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CommonMessageKeys.CurrencyMismatch, Currency, other.Currency);
        }
    }

    /// <inheritdoc />
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Amount:0.00} {Currency}");

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
