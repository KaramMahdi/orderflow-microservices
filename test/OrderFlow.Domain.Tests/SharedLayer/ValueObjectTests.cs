using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>Money (ADR-0003) and Address.</summary>
public sealed class ValueObjectTests
{
    private const string Usd = "USD";
    private const string Eur = "EUR";

    [Fact]
    public void MoneyNormalizesTheCurrencyAndComparesByValue()
    {
        var money = Money.Create(10.5m, " usd ");

        Assert.Equal(Usd, money.Currency);
        Assert.Equal(Money.Create(10.50m, Usd), money);
        Assert.NotEqual(Money.Create(10.5m, Eur), money);
    }

    [Fact]
    public void MoneyRejectsMoreThanTwoDecimalPlaces()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountTooPrecise, () => Money.Create(1.005m, Usd));
    }

    [Fact]
    public void MoneyRejectsANegativeAmount()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountNegative, () => Money.Create(-0.01m, Usd));
    }

    [Fact]
    public void MoneyRejectsAnAmountAboveTheColumnLimit()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountTooLarge, () => Money.Create(Money.MaxAmount + 0.01m, Usd));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDT")]
    [InlineData("U5D")]
    public void MoneyRejectsAnInvalidCurrency(string? currency)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.CurrencyCodeInvalid, () => Money.Create(1m, currency));
    }

    [Fact]
    public void MoneyAddsAndMultipliesInOneCurrency()
    {
        var sum = Money.Create(1.25m, Usd).Add(Money.Create(2m, Usd));

        Assert.Equal(Money.Create(3.25m, Usd), sum);
        Assert.Equal(Money.Create(9.75m, Usd), sum.Multiply(3));
    }

    [Fact]
    public void MoneyNeverCombinesTwoCurrencies()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.CurrencyMismatch, () => Money.Create(1m, Usd).Add(Money.Create(1m, Eur)));
    }

    [Fact]
    public void AddressTrimsAndNormalizesTheCountry()
    {
        var address = Address.Create(" 1 Main St ", " Amman ", "  ", "jo");

        Assert.Equal("1 Main St", address.Street);
        Assert.Equal("Amman", address.City);
        Assert.Null(address.PostalCode);
        Assert.Equal("JO", address.CountryCode);
        Assert.Equal(Address.Create("1 Main St", "Amman", null, "JO"), address);
    }

    [Fact]
    public void AddressRequiresAStreet()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.FieldRequired, () => Address.Create(" ", "Amman", null, "JO"));
    }

    [Fact]
    public void AddressRejectsALongCity()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.FieldTooLong, () => Address.Create("1 Main St", new string('a', Address.CityMaxLength + 1), null, "JO"));
    }

    [Fact]
    public void AddressRejectsAnInvalidCountry()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.CountryCodeInvalid, () => Address.Create("1 Main St", "Amman", null, "JOR"));
    }
}
