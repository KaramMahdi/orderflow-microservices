using Domain.Shared.Layer.DomainModel;
using Domain.Shared.Layer.Errors;
using Domain.Shared.Layer.Localization;

namespace Domain.Shared.Layer.ValueObjects;

/// <summary>
/// A postal address (glossary: the delivery address of an order, the address of a customer).
/// Immutable, created through <see cref="Create"/>.
/// </summary>
public sealed class Address : ValueObject
{
    /// <summary>The maximum length of <see cref="Street"/>.</summary>
    public const int StreetMaxLength = 200;

    /// <summary>The maximum length of <see cref="City"/>.</summary>
    public const int CityMaxLength = 100;

    /// <summary>The maximum length of <see cref="PostalCode"/>.</summary>
    public const int PostalCodeMaxLength = 20;

    /// <summary>The length of <see cref="CountryCode"/> (ISO 3166-1 alpha-2).</summary>
    public const int CountryCodeLength = 2;

    private Address(string street, string city, string? postalCode, string countryCode)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        CountryCode = countryCode;
    }

    /// <summary>The street and building.</summary>
    public string Street { get; }

    /// <summary>The city.</summary>
    public string City { get; }

    /// <summary>The postal code, when the country uses one.</summary>
    public string? PostalCode { get; }

    /// <summary>The country: two upper-case letters (ISO 3166-1 alpha-2).</summary>
    public string CountryCode { get; }

    /// <summary>Creates a validated address. Texts are trimmed.</summary>
    /// <param name="street">The street and building.</param>
    /// <param name="city">The city.</param>
    /// <param name="postalCode">The postal code, optional.</param>
    /// <param name="countryCode">The country code (ISO 3166-1 alpha-2, any case).</param>
    public static Address Create(string? street, string? city, string? postalCode, string? countryCode)
    {
        var validStreet = Check.Text(street, nameof(Street), StreetMaxLength, CommonMessageKeys.FieldRequired, CommonMessageKeys.FieldTooLong);
        var validCity = Check.Text(city, nameof(City), CityMaxLength, CommonMessageKeys.FieldRequired, CommonMessageKeys.FieldTooLong);
        var validPostalCode = Check.OptionalText(postalCode, nameof(PostalCode), PostalCodeMaxLength, CommonMessageKeys.FieldTooLong);
        var country = countryCode?.Trim().ToUpperInvariant();

        if (country is null || country.Length != CountryCodeLength || !country.All(char.IsAsciiLetterUpper))
        {
            throw new ErrorException(ErrorCode.ValidationFailed, CommonMessageKeys.CountryCodeInvalid, countryCode);
        }

        return new Address(validStreet, validCity, validPostalCode, country);
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return PostalCode;
        yield return CountryCode;
    }
}
