using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;
using Domain.Shared.Layer.ValueObjects;
using OrderFlow.Domain.Tests.TestSupport;

namespace OrderFlow.Domain.Tests.SharedLayer;

/// <summary>The remaining invariants of Money and Address: each one broken once.</summary>
public sealed class ValueObjectInvariantTests
{
    private const string Usd = "USD";
    private const string Street = "1 Main St";
    private const string City = "Amman";
    private const string Country = "JO";

    [Fact]
    public void EnsureSameCurrencyRejectsAnotherCurrency()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.CurrencyMismatch, () => Money.Create(1m, Usd).EnsureSameCurrency(Money.Create(1m, "JOD")));
    }

    [Fact]
    public void HasSameCurrencyComparesOnlyTheCurrency()
    {
        Assert.True(Money.Create(1m, Usd).HasSameCurrency(Money.Create(2m, Usd)));
        Assert.False(Money.Create(1m, Usd).HasSameCurrency(Money.Create(1m, "EUR")));
    }

    [Fact]
    public void AddAboveTheColumnLimitIsRejected()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountTooLarge, () => Money.Create(Money.MaxAmount, Usd).Add(Money.Create(0.01m, Usd)));
    }

    [Fact]
    public void MultiplyAboveTheColumnLimitIsRejected()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.MoneyAmountTooLarge, () => Money.Create(Money.MaxAmount, Usd).Multiply(2));
    }

    [Fact]
    public void MultiplyByANegativeQuantityIsAProgrammingError()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Money.Create(1m, Usd).Multiply(-1));
    }

    [Fact]
    public void TheLargestAmountAndZeroAreAccepted()
    {
        Assert.Equal(Money.MaxAmount, Money.Create(Money.MaxAmount, Usd).Amount);
        Assert.Equal(0m, Money.Zero(Usd).Amount);
    }

    [Fact]
    public void AddressRequiresACity()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.FieldRequired, () => Address.Create(Street, null, null, Country));
    }

    [Fact]
    public void AddressRejectsALongStreet()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.FieldTooLong, () => Address.Create(new string('s', Address.StreetMaxLength + 1), City, null, Country));
    }

    [Fact]
    public void AddressRejectsALongPostalCode()
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.FieldTooLong, () => Address.Create(Street, City, new string('9', Address.PostalCodeMaxLength + 1), Country));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("J1")]
    public void AddressRequiresATwoLetterCountry(string? country)
    {
        ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.CountryCodeInvalid, () => Address.Create(Street, City, null, country));
    }

    [Fact]
    public void TheErrorCarriesTheFieldNameAndTheLimit()
    {
        var exception = ErrorAssert.Throws(ErrorCode.ValidationFailed, CommonMessageKeys.FieldTooLong, () => Address.Create(Street, new string('c', Address.CityMaxLength + 1), null, Country));

        Assert.Equal([nameof(Address.City), Address.CityMaxLength], exception.Args);
    }
}
